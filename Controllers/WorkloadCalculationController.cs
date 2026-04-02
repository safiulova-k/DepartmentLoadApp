using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PortalMock;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.ViewModels.Workload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IAcademicPlanImportService _academicPlanImportService;

        public WorkloadCalculationController(
            DepartmentLoadDbContext context,
            IAcademicPlanImportService academicPlanImportService)
        {
            _context = context;
            _academicPlanImportService = academicPlanImportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? year)
        {
            var selectedYear = year ?? await _academicPlanImportService.GetLatestYearAsync() ?? DateTime.Now.Year;

            await _academicPlanImportService.EnsureYearImportedAsync(selectedYear);

            var rows = await _context.WorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            await Recalculate(rows);
            await _context.SaveChangesAsync();

            return View(new WorkloadTablePageViewModel
            {
                SelectedYear = selectedYear,
                Rows = rows
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(WorkloadTablePageViewModel model)
        {
            var ids = model.Rows.Select(x => x.Id).ToList();

            var dbRows = await _context.WorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Id)
                .ToListAsync();

            foreach (var postedRow in model.Rows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == postedRow.Id);
                if (dbRow == null)
                {
                    continue;
                }

                dbRow.HasExam = postedRow.HasExam;
                dbRow.HasCredit = postedRow.HasCredit;
                dbRow.HasCourseWork = postedRow.HasCourseWork;
                dbRow.HasCourseProject = postedRow.HasCourseProject;
            }

            await Recalculate(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = model.SelectedYear });
        }

        private async Task Recalculate(List<WorkloadRow> rows)
        {
            var lectureNorm = await GetNormAsync("Лекции");
            var practiceNorm = await GetNormAsync("Практические занятия");
            var labNorm = await GetNormAsync("Лабораторные работы");

            var consultationNorm = await GetNormAsync("Консультации");
            var consultationExamExtraNorm = await GetNormAsync("Доп. консультация к экзамену");

            var examNorm = await GetNormAsync("Экзамен");
            var creditNorm = await GetNormAsync("Зачет");
            var courseWorkNorm = await GetNormAsync("Курсовая работа");
            var courseProjectNorm = await GetNormAsync("Курсовой проект");

            foreach (var row in rows)
            {
                var cont = await _context.ContingentRows
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DirectionCode == row.DirectionCode);

                if (cont == null)
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.SubgroupCount = 0;

                    row.LectureTotalHours = 0;
                    row.PracticeTotalHours = 0;
                    row.LabTotalHours = 0;
                    row.ConsultationHours = 0;
                    row.ExamHours = 0;
                    row.CreditHours = 0;
                    row.CourseWorkHours = 0;
                    row.CourseProjectHours = 0;

                    continue;
                }

                row.StudentsCount = row.Course switch
                {
                    1 => cont.Course1Count,
                    2 => cont.Course2Count,
                    3 => cont.Course3Count,
                    4 => cont.Course4Count,
                    _ => 0
                };

                row.GroupCount = row.Course switch
                {
                    1 => cont.Course1Groups,
                    2 => cont.Course2Groups,
                    3 => cont.Course3Groups,
                    4 => cont.Course4Groups,
                    _ => 0
                };

                row.SubgroupCount = row.Course switch
                {
                    1 => cont.Course1Subgroups,
                    2 => cont.Course2Subgroups,
                    3 => cont.Course3Subgroups,
                    4 => cont.Course4Subgroups,
                    _ => 0
                };

                row.LectureTotalHours = CalculatePlanBasedHours(row.LecturePlanHours, lectureNorm, row);
                row.PracticeTotalHours = CalculatePlanBasedHours(row.PracticePlanHours, practiceNorm, row);
                row.LabTotalHours = CalculatePlanBasedHours(row.LabPlanHours, labNorm, row);

                row.ExamHours = CalculateOptionalHours(row.HasExam, examNorm, row);
                row.CreditHours = CalculateOptionalHours(row.HasCredit, creditNorm, row);
                row.CourseWorkHours = CalculateOptionalHours(row.HasCourseWork, courseWorkNorm, row);
                row.CourseProjectHours = CalculateOptionalHours(row.HasCourseProject, courseProjectNorm, row);

                row.ConsultationHours = CalculateConsultationHours(
                    row,
                    consultationNorm,
                    consultationExamExtraNorm);
            }
        }

        private decimal CalculatePlanBasedHours(decimal planHours, NormTime? norm, WorkloadRow row)
        {
            if (planHours <= 0 || norm == null)
            {
                return 0;
            }

            var multiplier = GetBaseValue(norm.CalculationBase, row);
            var result = planHours * multiplier * norm.Hours;

            return Math.Round(result, 0, MidpointRounding.AwayFromZero);
        }

        private decimal CalculateOptionalHours(bool isEnabled, NormTime? norm, WorkloadRow row)
        {
            if (!isEnabled || norm == null)
            {
                return 0;
            }

            var baseValue = GetBaseValue(norm.CalculationBase, row);
            var result = baseValue * norm.Hours;

            return Math.Round(result, 0, MidpointRounding.AwayFromZero);
        }

        private decimal CalculateConsultationHours(
            WorkloadRow row,
            NormTime? consultationNorm,
            NormTime? consultationExamExtraNorm)
        {
            decimal result = 0;

            if (consultationNorm != null)
            {
                decimal consultationBase = consultationNorm.CalculationBase switch
                {
                    WorkCalculationBase.FromLectureHoursTotal => row.GroupCount * row.LecturePlanHours,
                    _ => GetBaseValue(consultationNorm.CalculationBase, row)
                };

                result += consultationBase * consultationNorm.Hours;
            }

            if (row.HasExam && consultationExamExtraNorm != null)
            {
                var extraBase = GetBaseValue(consultationExamExtraNorm.CalculationBase, row);
                result += extraBase * consultationExamExtraNorm.Hours;
            }

            return Math.Round(result, 0, MidpointRounding.AwayFromZero);
        }

        private decimal GetBaseValue(WorkCalculationBase calculationBase, WorkloadRow row)
        {
            return calculationBase switch
            {
                WorkCalculationBase.PerStream => row.FlowCount,
                WorkCalculationBase.PerGroup => row.GroupCount,
                WorkCalculationBase.PerSubgroup => row.SubgroupCount,
                WorkCalculationBase.PerStudent => row.StudentsCount,
                WorkCalculationBase.PerWork => 1,
                WorkCalculationBase.FromLectureHoursTotal => row.GroupCount * row.LecturePlanHours,
                _ => 1
            };
        }

        private async Task<NormTime?> GetNormAsync(string workName)
        {
            return await _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);
        }
    }
}