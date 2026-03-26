using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.ViewModels.Contingent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class ContingentController : Controller
    {
        private readonly DepartmentLoadDbContext _context;

        public ContingentController(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rows = await _context.ContingentRows
                .OrderBy(x => x.Id)
                .ToListAsync();

            var model = new ContingentPageViewModel
            {
                Rows = rows
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ContingentPageViewModel model)
        {
            await UpsertRowsFromModel(model);

            TempData["SuccessMessage"] = "Таблица контингента сохранена";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ContingentPageViewModel model)
        {
            await UpsertRowsFromModel(model);

            _context.ContingentRows.Add(new ContingentRow
            {
                DirectionCode = string.Empty,
                IsBachelor = true,
                IsMaster = false,

                Course1Count = 0,
                Course2Count = 0,
                Course3Count = 0,
                Course4Count = 0,

                Course1Groups = 0,
                Course2Groups = 0,
                Course3Groups = 0,
                Course4Groups = 0,

                Course1Subgroups = 0,
                Course2Subgroups = 0,
                Course3Subgroups = 0,
                Course4Subgroups = 0,

                TotalCount = 0
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Направление добавлено";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, ContingentPageViewModel model)
        {
            await UpsertRowsFromModel(model);

            var entity = await _context.ContingentRows.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                _context.ContingentRows.Remove(entity);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Направление удалено";
            return RedirectToAction(nameof(Index));
        }

        private async Task UpsertRowsFromModel(ContingentPageViewModel model)
        {
            if (model?.Rows == null)
            {
                return;
            }

            NormalizeRows(model.Rows);

            var existingRows = await _context.ContingentRows.ToListAsync();

            foreach (var row in model.Rows)
            {
                var current = existingRows.FirstOrDefault(x => x.Id == row.Id);

                if (current == null)
                {
                    _context.ContingentRows.Add(new ContingentRow
                    {
                        DirectionCode = row.DirectionCode,
                        IsBachelor = row.IsBachelor,
                        IsMaster = row.IsMaster,

                        Course1Count = row.Course1Count,
                        Course2Count = row.Course2Count,
                        Course3Count = row.Course3Count,
                        Course4Count = row.Course4Count,

                        Course1Groups = row.Course1Groups,
                        Course2Groups = row.Course2Groups,
                        Course3Groups = row.Course3Groups,
                        Course4Groups = row.Course4Groups,

                        Course1Subgroups = row.Course1Subgroups,
                        Course2Subgroups = row.Course2Subgroups,
                        Course3Subgroups = row.Course3Subgroups,
                        Course4Subgroups = row.Course4Subgroups,

                        TotalCount = row.TotalCount
                    });
                }
                else
                {
                    current.DirectionCode = row.DirectionCode;
                    current.IsBachelor = row.IsBachelor;
                    current.IsMaster = row.IsMaster;

                    current.Course1Count = row.Course1Count;
                    current.Course2Count = row.Course2Count;
                    current.Course3Count = row.Course3Count;
                    current.Course4Count = row.Course4Count;

                    current.Course1Groups = row.Course1Groups;
                    current.Course2Groups = row.Course2Groups;
                    current.Course3Groups = row.Course3Groups;
                    current.Course4Groups = row.Course4Groups;

                    current.Course1Subgroups = row.Course1Subgroups;
                    current.Course2Subgroups = row.Course2Subgroups;
                    current.Course3Subgroups = row.Course3Subgroups;
                    current.Course4Subgroups = row.Course4Subgroups;

                    current.TotalCount = row.TotalCount;
                }
            }

            await _context.SaveChangesAsync();
        }

        private static void NormalizeRows(List<ContingentRow> rows)
        {
            foreach (var row in rows)
            {
                if (row.Course1Count < 0) row.Course1Count = 0;
                if (row.Course2Count < 0) row.Course2Count = 0;
                if (row.Course3Count < 0) row.Course3Count = 0;
                if (row.Course4Count < 0) row.Course4Count = 0;

                if (row.Course1Groups < 0) row.Course1Groups = 0;
                if (row.Course2Groups < 0) row.Course2Groups = 0;
                if (row.Course3Groups < 0) row.Course3Groups = 0;
                if (row.Course4Groups < 0) row.Course4Groups = 0;

                if (row.Course1Subgroups < 0) row.Course1Subgroups = 0;
                if (row.Course2Subgroups < 0) row.Course2Subgroups = 0;
                if (row.Course3Subgroups < 0) row.Course3Subgroups = 0;
                if (row.Course4Subgroups < 0) row.Course4Subgroups = 0;

                row.TotalCount =
                    row.Course1Count +
                    row.Course2Count +
                    row.Course3Count +
                    row.Course4Count;

                row.IsMaster = !row.IsBachelor;
            }
        }
    }
}