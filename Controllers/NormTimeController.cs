using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.NormTime;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class NormTimeController : Controller
    {
        private static List<NormTimeRowViewModel> _storage = CreateInitialData();

        [HttpGet]
        public IActionResult Index()
        {
            var model = new NormTimePageViewModel
            {
                Rows = _storage
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

            model.Id = _storage.Count == 0 ? 1 : _storage.Max(x => x.Id) + 1;

            if (string.IsNullOrWhiteSpace(model.UnitName))
            {
                model.UnitName = GetDefaultUnit(model.CalculationType);
            }

            _storage.Add(model);

            TempData["SuccessMessage"] = "Норма времени добавлена";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _storage.FirstOrDefault(x => x.Id == id);
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

            var current = _storage.FirstOrDefault(x => x.Id == model.Id);
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
            var current = _storage.FirstOrDefault(x => x.Id == id);
            if (current != null)
            {
                _storage.Remove(current);
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

        private static List<NormTimeRowViewModel> CreateInitialData()
        {
            return new List<NormTimeRowViewModel>
            {
                new()
                {
                    Id = 1,
                    WorkTypeName = "Лекции",
                    UnitName = "час/поток",
                    CalculationType = NormCalculationType.PerFlow,
                    HoursValue = 1,
                    Note = "Используется при расчете лекционных часов",
                    IsActive = true
                },
                new()
                {
                    Id = 2,
                    WorkTypeName = "Практические занятия",
                    UnitName = "час/группа",
                    CalculationType = NormCalculationType.PerGroup,
                    HoursValue = 1,
                    Note = "Используется при расчете практических занятий",
                    IsActive = true
                },
                new()
                {
                    Id = 3,
                    WorkTypeName = "Лабораторные занятия",
                    UnitName = "час/подгруппа",
                    CalculationType = NormCalculationType.PerSubgroup,
                    HoursValue = 1,
                    Note = "Используется при расчете лабораторных занятий",
                    IsActive = true
                },
                new()
                {
                    Id = 4,
                    WorkTypeName = "Руководство ВКР бакалавра",
                    UnitName = "час/студент",
                    CalculationType = NormCalculationType.PerStudent,
                    HoursValue = 1,
                    Note = "Норма задается вручную и потом участвует в расчете",
                    IsActive = true
                },
                new()
                {
                    Id = 5,
                    WorkTypeName = "Руководство ВКР магистра",
                    UnitName = "час/студент",
                    CalculationType = NormCalculationType.PerStudent,
                    HoursValue = 1,
                    Note = "Норма задается вручную и потом участвует в расчете",
                    IsActive = true
                }
            };
        }
    }
}