using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.ViewModels.Workload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DepartmentLoadApp.Services;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly CalculationImportService _importService;

        public WorkloadCalculationController(
            DepartmentLoadDbContext context,
            CalculationImportService importService)
        {
            _context = context;
            _importService = importService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await LoadRowsAsync(selectedYear);

            await RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return View(new WorkloadTablePageViewModel
            {
                SelectedYearStart = selectedYearStart,
                SelectedYear = selectedYear,
                AvailableYearStarts = AcademicYearResolver.BuildAvailableStartYears(selectedYearStart),
                Rows = rows
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromAcademicPlan(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);

            await _importService.ImportAllAsync(selectedYearStart);

            return RedirectToAction(nameof(Index), new { startYear = selectedYearStart });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(WorkloadTablePageViewModel model)
        {
            var ids = model.Rows.Select(x => x.Id).ToList();

            var dbRows = await _context.WorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            await RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { startYear = model.SelectedYearStart });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await LoadRowsAsync(selectedYear);
            await RecalculateAsync(rows);

            var content = ExcelExportHelper.ExportWorkload(rows);
            var fileName = $"Расчет_дисциплин_{selectedYear}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task<List<WorkloadRow>> LoadRowsAsync(string year)
        {
            return await _context.WorkloadRows
                .Where(x => x.AcademicYear == year)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();
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
            var rgrNorm = await GetNormAsync("РГР");

            var contingentMap = await _context.ContingentRows
                .AsNoTracking()
                .ToDictionaryAsync(x => x.DirectionCode);

            var flows = await _context.StudentFlows
                .AsNoTracking()
                .ToListAsync();

            foreach (var row in rows)
            {
                if (!contingentMap.TryGetValue(row.DirectionCode, out var contingent))
                {
                    ResetCalculatedFields(row);
                    continue;
                }

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);
                row.SubgroupCount = CalculationHelper.GetSubgroupsByCourse(contingent, row.Course);

                row.FlowCount = flows.Count(x =>
                    x.AcademicYear == row.AcademicYear &&
                    x.DirectionCode == row.DirectionCode &&
                    x.Course == row.Course);

                if (row.FlowCount <= 0)
                {
                    row.FlowCount = row.GroupCount > 0 ? 1 : 0;
                }

                row.LectureTotalHours = NormCalculationHelper.CalculatePlanHours(row.LecturePlanHours, lectureNorm, row);
                row.PracticeTotalHours = NormCalculationHelper.CalculatePlanHours(row.PracticePlanHours, practiceNorm, row);
                row.LabTotalHours = NormCalculationHelper.CalculatePlanHours(row.LabPlanHours, labNorm, row);

                row.ExamHours = NormCalculationHelper.CalculateOptionalHours(row.HasExam, examNorm, row);
                row.CreditHours = NormCalculationHelper.CalculateOptionalHours(row.HasCredit, creditNorm, row);
                row.CourseWorkHours = NormCalculationHelper.CalculateOptionalHours(row.HasCourseWork, courseWorkNorm, row);
                row.CourseProjectHours = NormCalculationHelper.CalculateOptionalHours(row.HasCourseProject, courseProjectNorm, row);
                row.RgrHours = NormCalculationHelper.CalculateOptionalHours(row.HasRgr, rgrNorm, row);

                row.ConsultationHours = NormCalculationHelper.CalculateConsultationHours(
                    row,
                    consultationNorm,
                    consultationExamExtraNorm);
            }
        }

        private static void ResetCalculatedFields(WorkloadRow row)
        {
            row.StudentsCount = 0;
            row.FlowCount = 0;
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
            row.RgrHours = 0;
        }

        private Task<NormTime?> GetNormAsync(string workName)
        {
            return _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);
        }

        private static bool IsDisciplineRecord(string? index)
        {
            if (string.IsNullOrWhiteSpace(index))
                return false;

            var normalized = index.Trim().ToUpperInvariant();

            return normalized.StartsWith("Б1.") || normalized.StartsWith("ФТД");
        }

        private static string GetEducationFormName(DepartmentLoadApp.Models.Core.AcademicPlan plan)
        {
            return plan.EducationForm.ToString();
        }
    }
}