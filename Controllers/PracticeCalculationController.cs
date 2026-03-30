using DepartmentLoadApp.Data;
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

        public PracticeCalculationController(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;

            await EnsureDefaultRowsAsync(selectedYear);

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

                var norm = await GetNorm(row.PracticeName);
                var coefficient = norm?.Hours ?? 0m;

                row.TotalHours = row.WeeksCount * row.StudentsCount * coefficient;
            }
        }

        private async Task<NormTime?> GetNorm(string practiceName)
        {
            return await _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == practiceName);
        }

        private async Task EnsureDefaultRowsAsync(int year)
        {
            var hasRows = await _context.PracticeWorkloadRows
                .AnyAsync(x => x.PlanYear == year);

            if (hasRows)
            {
                return;
            }

            var rows = new List<PracticeWorkloadRow>
            {
                new() { PlanYear = year, DirectionCode = "09.03.03", DirectionName = "09.03.03", Course = 4, PracticeName = "НИР", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.03.04", DirectionName = "09.03.04", Course = 4, PracticeName = "НИР", WeeksCount = 0, TotalHours = 0 },

                new() { PlanYear = year, DirectionCode = "09.03.03", DirectionName = "09.03.03", Course = 1, PracticeName = "Учебная практика", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.03.04", DirectionName = "09.03.04", Course = 1, PracticeName = "Учебная практика", WeeksCount = 0, TotalHours = 0 },

                new() { PlanYear = year, DirectionCode = "09.03.03", DirectionName = "09.03.03", Course = 3, PracticeName = "Научно-исследовательская работа", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.03.04", DirectionName = "09.03.04", Course = 3, PracticeName = "Научно-исследовательская работа", WeeksCount = 0, TotalHours = 0 },

                new() { PlanYear = year, DirectionCode = "09.03.03", DirectionName = "09.03.03", Course = 4, PracticeName = "Технологическая практика", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.03.04", DirectionName = "09.03.04", Course = 4, PracticeName = "Технологическая практика", WeeksCount = 0, TotalHours = 0 },

                new() { PlanYear = year, DirectionCode = "09.03.03", DirectionName = "09.03.03", Course = 4, PracticeName = "Преддипломная практика", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.03.04", DirectionName = "09.03.04", Course = 4, PracticeName = "Преддипломная практика", WeeksCount = 0, TotalHours = 0 },

                new() { PlanYear = year, DirectionCode = "09.04.03 БИ", DirectionName = "09.04.03 БИ", Course = 1, PracticeName = "Ознакомительная практика", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.04.03 БИ", DirectionName = "09.04.03 БИ", Course = 2, PracticeName = "НИРМ", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.04.03 БИ", DirectionName = "09.04.03 БИ", Course = 2, PracticeName = "Преддипломная практика", WeeksCount = 0, TotalHours = 0 },

                new() { PlanYear = year, DirectionCode = "09.04.03", DirectionName = "09.04.03", Course = 1, PracticeName = "Ознакомительная практика", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.04.03", DirectionName = "09.04.03", Course = 2, PracticeName = "НИРМ", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.04.03", DirectionName = "09.04.03", Course = 2, PracticeName = "Преддипломная практика", WeeksCount = 0, TotalHours = 0 },

                new() { PlanYear = year, DirectionCode = "09.04.04", DirectionName = "09.04.04", Course = 1, PracticeName = "Ознакомительная практика", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.04.04", DirectionName = "09.04.04", Course = 2, PracticeName = "НИРМ", WeeksCount = 0, TotalHours = 0 },
                new() { PlanYear = year, DirectionCode = "09.04.04", DirectionName = "09.04.04", Course = 2, PracticeName = "Преддипломная практика", WeeksCount = 0, TotalHours = 0 }
            };

            await _context.PracticeWorkloadRows.AddRangeAsync(rows);
            await _context.SaveChangesAsync();
        }
    }
}