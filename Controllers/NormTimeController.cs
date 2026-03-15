using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.NormTime;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class NormTimeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var model = new NormTimePageViewModel
            {
                Rows = AppMemoryStore.NormTimes
                    .OrderBy(x => x.WorkTypeName)
                    .ToList()
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
        public IActionResult Create(NormTimeRowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Id = AppMemoryStore.NormTimes.Count == 0
                ? 1
                : AppMemoryStore.NormTimes.Max(x => x.Id) + 1;

            if (string.IsNullOrWhiteSpace(model.UnitName))
            {
                model.UnitName = GetDefaultUnit(model.CalculationType);
            }

            AppMemoryStore.NormTimes.Add(model);

            TempData["SuccessMessage"] = "Норма времени добавлена";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = AppMemoryStore.NormTimes.FirstOrDefault(x => x.Id == id);
            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NormTimeRowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var current = AppMemoryStore.NormTimes.FirstOrDefault(x => x.Id == model.Id);
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

            TempData["SuccessMessage"] = "Норма времени обновлена";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var current = AppMemoryStore.NormTimes.FirstOrDefault(x => x.Id == id);
            if (current != null)
            {
                AppMemoryStore.NormTimes.Remove(current);
                TempData["SuccessMessage"] = "Норма времени удалена";
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