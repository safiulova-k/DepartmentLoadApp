using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Integration.GiaImport;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.ViewModels.Gia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class GiaCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IGiaWorkloadImportService _giaWorkloadImportService;

        public GiaCalculationController(
            DepartmentLoadDbContext context,
            IGiaWorkloadImportService giaWorkloadImportService)
        {
            _context = context;
            _giaWorkloadImportService = giaWorkloadImportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? year)
        {
            List<GiaWorkloadRow> rows;
            string selectedYear;

            if (string.IsNullOrWhiteSpace(year))
            {
                rows = await _context.GiaWorkloadRows
                    .OrderBy(x => x.Course)
                    .ThenBy(x => x.DirectionCode)
                    .ThenBy(x => x.GiaSection)
                    .ThenBy(x => x.WorkName)
                    .ToListAsync();

                selectedYear = rows.FirstOrDefault()?.PlanYear
                               ?? AcademicYearHelper.GetCurrentAcademicYear();
            }
            else
            {
                selectedYear = year;

                await _giaWorkloadImportService.EnsureYearImportedAsync(selectedYear);

                rows = await _context.GiaWorkloadRows
                    .Where(x => x.PlanYear == selectedYear)
                    .OrderBy(x => x.Course)
                    .ThenBy(x => x.DirectionCode)
                    .ThenBy(x => x.GiaSection)
                    .ThenBy(x => x.WorkName)
                    .ToListAsync();
            }

            await RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return View(new GiaWorkloadPageViewModel
            {
                SelectedYear = selectedYear,
                Rows = rows
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(GiaWorkloadPageViewModel model)
        {
            var ids = model.Rows.Select(x => x.Id).ToList();

            var dbRows = await _context.GiaWorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            foreach (var row in model.Rows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == row.Id);
                if (dbRow == null)
                {
                    continue;
                }

                if (dbRow.WorkName == "Консультация к госэкзамену")
                {
                    dbRow.ManualHours = row.ManualHours;
                }
            }

            await RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = model.SelectedYear });
        }

        private async Task RecalculateAsync(List<GiaWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x => x.CategoryName == "ГИА")
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

                NormalizeGiaWorkNameByQualification(row, contingent);

                row.TotalHours = CalculateGiaHours(row, norms);
            }
        }

        private static void NormalizeGiaWorkNameByQualification(GiaWorkloadRow row, ContingentRow contingent)
        {
            if (row.WorkName != "Руководство ВКР")
            {
                return;
            }

            row.WorkName = contingent.IsMaster
                ? "Руководство ВКР магистра"
                : "Руководство ВКР бакалавра";
        }

        private decimal CalculateGiaHours(GiaWorkloadRow row, List<NormTime> norms)
        {
            if (row.WorkName == "Консультация к госэкзамену")
            {
                return CalculationHelper.RoundHours(row.ManualHours);
            }

            var norm = norms.FirstOrDefault(x => x.WorkName == row.WorkName);
            if (norm == null)
            {
                return 0;
            }

            var result = CalculationHelper.CalculateByNorm(
                calculationBase: norm.CalculationBase,
                coefficient: norm.Hours,
                studentsCount: row.StudentsCount,
                groupCount: row.GroupCount);

            return CalculationHelper.RoundHours(result);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string? year)
        {
            var selectedYear = string.IsNullOrWhiteSpace(year)
                ? AcademicYearHelper.GetCurrentAcademicYear()
                : year;

            await _giaWorkloadImportService.EnsureYearImportedAsync(selectedYear);

            var rows = await _context.GiaWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            await RecalculateAsync(rows);

            var content = ExcelExportHelper.ExportGia(rows);
            var fileName = $"Расчет_ГИА_{selectedYear}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}