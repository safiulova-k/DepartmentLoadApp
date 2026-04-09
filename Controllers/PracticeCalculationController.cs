using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.ViewModels.Practice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DepartmentLoadApp.Services;
namespace DepartmentLoadApp.Controllers
{
    public class PracticeCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly CalculationImportService _importService;

        public PracticeCalculationController(
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

            var rows = await _context.PracticeWorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            await RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return View(new PracticeWorkloadPageViewModel
            {
                SelectedYear = selectedYear,
                SelectedYearStart = selectedYearStart,
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
        public async Task<IActionResult> Save(PracticeWorkloadPageViewModel model)
        {
            var ids = model.Rows.Select(x => x.Id).ToList();

            var dbRows = await _context.PracticeWorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            foreach (var row in model.Rows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == row.Id);
                if (dbRow == null)
                    continue;

                dbRow.WeeksCount = row.WeeksCount;
            }

            await RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { startYear = model.SelectedYearStart });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await _context.PracticeWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            await RecalculateAsync(rows);

            var content = ExcelExportHelper.ExportPractice(rows);
            var fileName = $"Расчет_практик_{selectedYear}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task RecalculateAsync(List<PracticeWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.CategoryName) &&
                    (EF.Functions.ILike(x.CategoryName, "%практи%") ||
                     EF.Functions.ILike(x.CategoryName, "%науч%")))
                .ToListAsync();

            var contingentMap = await _context.ContingentRows
                .AsNoTracking()
                .ToDictionaryAsync(x => NormalizeText(x.DirectionCode));

            foreach (var row in rows)
            {
                if (!contingentMap.TryGetValue(NormalizeText(row.DirectionCode), out var contingent))
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.TotalHours = 0;
                    continue;
                }

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);

                var norm = FindPracticeNorm(norms, row.PracticeName);

                if (norm == null || row.WeeksCount <= 0 || norm.Hours <= 0)
                {
                    row.TotalHours = 0;
                    continue;
                }

                var result = CalculationHelper.CalculateByNorm(
                    calculationBase: norm.CalculationBase,
                    coefficient: norm.Hours,
                    studentsCount: row.StudentsCount,
                    groupCount: row.GroupCount,
                    weeksCount: row.WeeksCount);

                row.TotalHours = CalculationHelper.RoundHours(result);
            }
        }

        private static NormTime? FindPracticeNorm(List<NormTime> norms, string practiceName)
        {
            var target = NormalizePracticeKey(practiceName);

            return norms.FirstOrDefault(x => NormalizePracticeKey(x.WorkName) == target)
                ?? norms.FirstOrDefault(x => IsSamePracticeType(x.WorkName, practiceName));
        }

        private static bool IsSamePracticeType(string? left, string? right)
        {
            var a = NormalizePracticeKey(left);
            var b = NormalizePracticeKey(right);

            if (a == b)
                return true;

            if (a.Contains("технологическ") && b.Contains("технологическ"))
                return true;

            if (a.Contains("преддиплом") && b.Contains("преддиплом"))
                return true;

            if (a.Contains("ознаком") && b.Contains("ознаком"))
                return true;

            if (a == "нир" && b == "нир")
                return true;

            if (a.Contains("научно-исследователь") && b.Contains("научно-исследователь"))
                return true;

            if (a.Contains("учебн") && b.Contains("учебн"))
                return true;

            return false;
        }

        private static string NormalizePracticeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Replace("бакалавров", "")
                .Replace("магистров", "")
                .Replace("(учебная)", "")
                .Replace("(производственная)", "");

            return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return string.Join(' ', value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static bool IsPracticeRecord(string? index)
        {
            if (string.IsNullOrWhiteSpace(index))
                return false;

            var normalized = index.Trim().ToUpperInvariant();
            return normalized.StartsWith("Б2");
        }

        private static string NormalizePracticeName(string? sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                return string.Empty;

            var value = sourceName.Trim().ToLowerInvariant();

            if (value.Contains("ознаком"))
                return "Ознакомительная практика";

            if (value.Contains("технолог") || value.Contains("производствен"))
                return "Технологическая практика";

            if (value.Contains("преддиплом") && value.Contains("магистр"))
                return "Преддипломная практика магистров";

            if (value.Contains("преддиплом"))
                return "Преддипломная практика бакалавров";

            if (value.Contains("нирм"))
                return "НИРМ";

            if (value.Contains("научно-исследовательская"))
                return "Научно-исследовательская работа";

            if (value == "нир" || value.Contains(" нир"))
                return "НИР";

            if (value.Contains("учебн"))
                return "Учебная практика";

            return sourceName.Trim();
        }

        private static string GetEducationFormName(DepartmentLoadApp.Models.Core.AcademicPlan plan)
        {
            return plan.EducationForm.ToString();
        }
    }
}