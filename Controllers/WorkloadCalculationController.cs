using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
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

        public WorkloadCalculationController(
            DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? year)
        {
            List<WorkloadRow> rows;
            string selectedYear;

            if (string.IsNullOrWhiteSpace(year))
            {
                rows = await _context.WorkloadRows
                    .OrderBy(x => x.Course)
                    .ThenBy(x => x.SemesterName)
                    .ThenBy(x => x.DisciplineName)
                    .ToListAsync();

                selectedYear = rows.FirstOrDefault()?.AcademicYear
                               ?? AcademicYearHelper.GetCurrentAcademicYear();
            }
            else
            {
                selectedYear = year;

                rows = await _context.WorkloadRows
                    .Where(x => x.AcademicYear == selectedYear)
                    .OrderBy(x => x.Course)
                    .ThenBy(x => x.SemesterName)
                    .ThenBy(x => x.DisciplineName)
                    .ToListAsync();
            }

            return View(new WorkloadTablePageViewModel
            {
                SelectedYear = selectedYear,
                Rows = rows
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromAcademicPlan(string year)
        {
            if (string.IsNullOrWhiteSpace(year))
            {
                year = AcademicYearHelper.GetCurrentAcademicYear();
            }


            var rows = await _context.WorkloadRows
                .Where(x => x.AcademicYear == year)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            await RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            await RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = model.SelectedYear });
        }

        private async Task RecalculateAsync(List<WorkloadRow> rows)
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

            var contingentMap = await _context.ContingentRows
                .AsNoTracking()
                .ToDictionaryAsync(x => x.DirectionCode);

            foreach (var row in rows)
            {
                if (!contingentMap.TryGetValue(row.DirectionCode, out var contingent))
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

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);
                row.SubgroupCount = CalculationHelper.GetSubgroupsByCourse(contingent, row.Course);

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

            return CalculationHelper.RoundHours(result);
        }

        private decimal CalculateOptionalHours(bool isEnabled, NormTime? norm, WorkloadRow row)
        {
            if (!isEnabled || norm == null)
            {
                return 0;
            }

            var baseValue = GetBaseValue(norm.CalculationBase, row);
            var result = baseValue * norm.Hours;

            return CalculationHelper.RoundHours(result);
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

            return CalculationHelper.RoundHours(result);
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

        private Task<NormTime?> GetNormAsync(string workName)
        {
            return _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? year)
        {
            var selectedYear = string.IsNullOrWhiteSpace(year)
                ? AcademicYearHelper.GetCurrentAcademicYear()
                : year;

            var rows = await _context.WorkloadRows
                .AsNoTracking()
                .Where(x => x.AcademicYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            await RecalculateAsync(rows);

            var content = ExcelExportHelper.ExportWorkload(rows);
            var fileName = $"Расчет_дисциплин_{selectedYear}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}