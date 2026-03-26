using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.NormTime;
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
            var rows = await _context.NormTimes
                .OrderBy(x => x.WorkTypeName)
                .Select(x => new NormTimeRowViewModel
                {
                    Id = x.Id,
                    WorkTypeName = x.WorkTypeName,
                    CalculationType = x.CalculationType,
                    UnitName = x.UnitName,
                    HoursValue = x.HoursValue,
                    Note = x.Note,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            var model = new NormTimePageViewModel
            {
                Rows = rows
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new NormTimeRowViewModel
            {
                IsActive = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NormTimeRowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.UnitName))
            {
                model.UnitName = GetDefaultUnit(model.CalculationType);
            }

            var entity = new NormTime
            {
                WorkTypeName = model.WorkTypeName,
                CalculationType = model.CalculationType,
                UnitName = model.UnitName,
                HoursValue = model.HoursValue < 0 ? 0 : model.HoursValue,
                Note = model.Note,
                IsActive = model.IsActive
            };

            _context.NormTimes.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Норма времени добавлена";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.NormTimes.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Запись не найдена";
                return RedirectToAction(nameof(Index));
            }

            var model = new NormTimeRowViewModel
            {
                Id = entity.Id,
                WorkTypeName = entity.WorkTypeName,
                CalculationType = entity.CalculationType,
                UnitName = entity.UnitName,
                HoursValue = entity.HoursValue,
                Note = entity.Note,
                IsActive = entity.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NormTimeRowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var current = await _context.NormTimes.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (current == null)
            {
                TempData["ErrorMessage"] = "Запись не найдена";
                return RedirectToAction(nameof(Index));
            }

            current.WorkTypeName = model.WorkTypeName;
            current.CalculationType = model.CalculationType;
            current.UnitName = string.IsNullOrWhiteSpace(model.UnitName)
                ? GetDefaultUnit(model.CalculationType)
                : model.UnitName;
            current.HoursValue = model.HoursValue < 0 ? 0 : model.HoursValue;
            current.Note = model.Note;
            current.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Норма времени обновлена";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var current = await _context.NormTimes.FirstOrDefaultAsync(x => x.Id == id);
            if (current != null)
            {
                _context.NormTimes.Remove(current);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Норма времени удалена";
            }
            else
            {
                TempData["ErrorMessage"] = "Запись не найдена";
            }

            return RedirectToAction(nameof(Index));
        }

        private static string GetDefaultUnit(NormCalculationType type)
        {
            return type switch
            {
                NormCalculationType.PerStudent => "час/студент",
                NormCalculationType.PerGroup => "час/группа",
                NormCalculationType.PerSubgroup => "час/подгруппа",
                NormCalculationType.PerFlow => "час/поток",
                NormCalculationType.Fixed => "фиксировано",
                _ => string.Empty
            };
        }
    }
}