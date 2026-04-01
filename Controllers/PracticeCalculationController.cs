using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PracticeMock;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Enums;
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

            await Recalculate(rows);
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

            await Recalculate(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = model.SelectedYear });
        }

        private async Task Recalculate(List<PracticeWorkloadRow> rows)
        {
            foreach (var row in rows)
            {
                var cont = await _context.ContingentRows
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DirectionCode == row.DirectionCode);

                if (cont == null)
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.TotalHours = 0;
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

                var norm = await GetNorm(row.PracticeName);

                if (norm == null)
                {
                    row.TotalHours = 0;
                    continue;
                }

                var rawHours = norm.CalculationBase switch
                {
                    WorkCalculationBase.PerStudent => row.WeeksCount * row.StudentsCount * norm.Hours,
                    WorkCalculationBase.PerGroup => row.WeeksCount * row.GroupCount * norm.Hours,
                    _ => row.WeeksCount * row.StudentsCount * norm.Hours
                };

                row.TotalHours = (int)Math.Round(rawHours, MidpointRounding.AwayFromZero);
            }
        }

        private async Task<NormTime?> GetNorm(string practiceName)
        {
            return await _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == practiceName);
        }
    }
}