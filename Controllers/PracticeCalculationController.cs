using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PracticeMock;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.ViewModels.Practice;
using DepartmentLoadApp.Helpers;
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
        public async Task<IActionResult> Index(int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;

            await _practiceWorkloadImportService.EnsureYearImportedAsync(selectedYear);

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
                Rows = rows
            });
        }

        [HttpPost]
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

            foreach (var row in rows)
            {
                var contingent = await _context.ContingentRows
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DirectionCode == row.DirectionCode);

                if (contingent == null)
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.TotalHours = 0;
                    continue;
                }

                row.StudentsCount = GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = GetGroupsByCourse(contingent, row.Course);

                var norm = norms.FirstOrDefault(x => x.WorkName == row.PracticeName);
                if (norm == null)
                {
                    row.TotalHours = 0;
                    continue;
                }

                var result = CalculatePracticeHours(row, norm);
                row.TotalHours = RoundHours(result);
            }
        }

        private static decimal CalculatePracticeHours(PracticeWorkloadRow row, NormTime norm)
        {
            return CalculateByNorm(
                calculationBase: norm.CalculationBase,
                coefficient: norm.Hours,
                studentsCount: row.StudentsCount,
                groupCount: row.GroupCount,
                subgroupCount: 0,
                streamCount: 0,
                weeksCount: row.WeeksCount,
                planHours: 0);
        }

        private static decimal CalculateByNorm(
            WorkCalculationBase calculationBase,
            decimal coefficient,
            int studentsCount,
            int groupCount,
            int subgroupCount,
            int streamCount,
            int weeksCount,
            decimal planHours)
        {
            return calculationBase switch
            {
                WorkCalculationBase.PerStudent => weeksCount * studentsCount * coefficient,
                WorkCalculationBase.PerGroup => weeksCount * groupCount * coefficient,
                WorkCalculationBase.PerSubgroup => weeksCount * subgroupCount * coefficient,
                WorkCalculationBase.PerStream => weeksCount * streamCount * coefficient,

                // Для практик сейчас не используется, но оставляю для общей логики
                WorkCalculationBase.PerWork => coefficient,
                WorkCalculationBase.FromLectureHoursTotal => planHours * coefficient,

                _ => 0
            };
        }

        private static int GetStudentsByCourse(ContingentRow contingent, int course)
        {
            return course switch
            {
                1 => contingent.Course1Count,
                2 => contingent.Course2Count,
                3 => contingent.Course3Count,
                4 => contingent.Course4Count,
                _ => 0
            };
        }

        private static int GetGroupsByCourse(ContingentRow contingent, int course)
        {
            return course switch
            {
                1 => contingent.Course1Groups,
                2 => contingent.Course2Groups,
                3 => contingent.Course3Groups,
                4 => contingent.Course4Groups,
                _ => 0
            };
        }

        private static decimal RoundHours(decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;

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