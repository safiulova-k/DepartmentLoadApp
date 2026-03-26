using DepartmentLoadApp.Data;
using DepartmentLoadApp.ViewModels.NormTime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class NormTimeController : Controller
    {
        private readonly DepartmentLoadDbContext _context;

        public NormTimeController(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _context.NormTimes
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Select(x => new NormTimeRowViewModel
                {
                    Id = x.Id,
                    WorkName = x.WorkName,
                    CategoryName = x.CategoryName,
                    CalculationBase = x.CalculationBase,
                    Hours = x.Hours
                })
                .ToListAsync();

            var model = new NormTimePageViewModel
            {
                Items = items
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(NormTimePageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var ids = model.Items.Select(x => x.Id).ToList();

            var dbItems = await _context.NormTimes
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            foreach (var dbItem in dbItems)
            {
                var postedItem = model.Items.First(x => x.Id == dbItem.Id);

                dbItem.CalculationBase = postedItem.CalculationBase;
                dbItem.Hours = postedItem.Hours;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Нормы времени сохранены";
            return RedirectToAction(nameof(Index));
        }
    }
}