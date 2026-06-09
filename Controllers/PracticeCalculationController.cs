using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models.Practice;
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
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            await _importService.ImportAllAsync(selectedYearStart);

            var rows = await LoadRowsAsync(selectedYear, asNoTracking: false);

            await _practiceCalculationService.RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { startYear = selectedYearStart });
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

        private async Task<List<PracticeWorkloadRow>> LoadRowsAsync(
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