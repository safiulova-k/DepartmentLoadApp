using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Integration.PracticeImport;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.ViewModels.Practice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class PracticeCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IPracticeWorkloadImportService _practiceWorkloadImportService;

        public PracticeCalculationController(
            DepartmentLoadDbContext context,
            IPracticeWorkloadImportService practiceWorkloadImportService)
        {
            _context = context;
            _practiceWorkloadImportService = practiceWorkloadImportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? year)
        {
            List<PracticeWorkloadRow> rows;
            string selectedYear;

            if (string.IsNullOrWhiteSpace(year))
            {
                rows = await _context.PracticeWorkloadRows
                    .OrderBy(x => x.Course)
                    .ThenBy(x => x.DirectionCode)
                    .ThenBy(x => x.PracticeName)
                    .ToListAsync();

                selectedYear = rows.FirstOrDefault()?.PlanYear
                               ?? AcademicYearHelper.GetCurrentAcademicYear();
            }
            else
            {
                selectedYear = year;

                await _practiceWorkloadImportService.EnsureYearImportedAsync(selectedYear);

                rows = await _context.PracticeWorkloadRows
                    .Where(x => x.PlanYear == selectedYear)
                    .OrderBy(x => x.Course)
                    .ThenBy(x => x.DirectionCode)
                    .ThenBy(x => x.PracticeName)
                    .ToListAsync();
            }

            await RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return View(new PracticeWorkloadPageViewModel
            {
                SelectedYear = selectedYear,
                Rows = rows
            });
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
                {
                    continue;
                }

                dbRow.WeeksCount = row.WeeksCount;
            }

            await RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = model.SelectedYear });
        }

        private async Task RecalculateAsync(List<PracticeWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x => x.CategoryName == "Практика" || x.CategoryName == "Научная работа")
                .ToListAsync();

            var contingentMap = await _context.ContingentRows
                .AsNoTracking()
                .ToDictionaryAsync(x => x.DirectionCode);

            foreach (var row in rows)
            {
                if (!contingentMap.TryGetValue(row.DirectionCode, out var contingent))
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.TotalHours = 0;
                    continue;
                }

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);

                var norm = norms.FirstOrDefault(x => x.WorkName == row.PracticeName);
                if (norm == null)
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

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? year)
        {
            var selectedYear = string.IsNullOrWhiteSpace(year)
                ? AcademicYearHelper.GetCurrentAcademicYear()
                : year;

            await _practiceWorkloadImportService.EnsureYearImportedAsync(selectedYear);

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
    }
}