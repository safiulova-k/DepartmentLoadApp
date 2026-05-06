using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Services;
using DepartmentLoadApp.ViewModels.Practice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class PracticeCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly CalculationImportService _importService;
        private readonly PracticeCalculationService _practiceCalculationService;

        public PracticeCalculationController(
            DepartmentLoadDbContext context,
            CalculationImportService importService,
            PracticeCalculationService practiceCalculationService)
        {
            _context = context;
            _importService = importService;
            _practiceCalculationService = practiceCalculationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await LoadRowsAsync(selectedYear, asNoTracking: true);

            await _practiceCalculationService.RecalculateAsync(rows);

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
            var inputRows = model.Rows ?? new();

            var ids = inputRows
                .Select(x => x.Id)
                .ToList();

            var dbRows = await _context.PracticeWorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            foreach (var row in inputRows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == row.Id);

                if (dbRow == null)
                    continue;

                dbRow.WeeksCount = row.WeeksCount;
            }

            await _practiceCalculationService.RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { startYear = model.SelectedYearStart });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await LoadRowsAsync(selectedYear, asNoTracking: true);

            await _practiceCalculationService.RecalculateAsync(rows);

            var content = ExcelExportHelper.ExportPractice(rows);
            var fileName = $"Расчет_практик_{selectedYear}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task<List<Models.Practice.PracticeWorkloadRow>> LoadRowsAsync(
            string selectedYear,
            bool asNoTracking)
        {
            var query = _context.PracticeWorkloadRows
                .Where(x => x.PlanYear == selectedYear);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();
        }
    }
}