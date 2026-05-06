using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Services;
using DepartmentLoadApp.ViewModels.Gia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class GiaCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly CalculationImportService _importService;
        private readonly GiaCalculationService _giaCalculationService;

        public GiaCalculationController(
            DepartmentLoadDbContext context,
            CalculationImportService importService,
            GiaCalculationService giaCalculationService)
        {
            _context = context;
            _importService = importService;
            _giaCalculationService = giaCalculationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await LoadRowsAsync(selectedYear, asNoTracking: true);

            await _giaCalculationService.RecalculateAsync(rows);

            return View(new GiaWorkloadPageViewModel
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
        public async Task<IActionResult> Save(GiaWorkloadPageViewModel model)
        {
            var inputRows = model.Rows ?? new();

            var ids = inputRows
                .Select(x => x.Id)
                .ToList();

            var dbRows = await _context.GiaWorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            foreach (var row in inputRows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == row.Id);

                if (dbRow == null)
                    continue;

                if (dbRow.WorkName == "Консультация к госэкзамену")
                {
                    dbRow.ManualHours = row.ManualHours;
                }
            }

            await _giaCalculationService.RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { startYear = model.SelectedYearStart });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await LoadRowsAsync(selectedYear, asNoTracking: true);

            await _giaCalculationService.RecalculateAsync(rows);

            var content = ExcelExportHelper.ExportGia(rows);
            var fileName = $"Расчет_ГИА_{selectedYear}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task<List<GiaWorkloadRow>> LoadRowsAsync(
            string selectedYear,
            bool asNoTracking)
        {
            var query = _context.GiaWorkloadRows
                .Where(x => x.PlanYear == selectedYear);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();
        }
    }
}