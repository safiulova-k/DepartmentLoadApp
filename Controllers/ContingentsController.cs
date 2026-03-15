using DepartmentLoadApp.Models.Contingent;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class ContingentController : Controller
    {
        private static ContingentPageViewModel _storage = CreateInitialModel();

        [HttpGet]
        public IActionResult Index()
        {
            return View(_storage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(ContingentPageViewModel model)
        {
            if (model == null || model.Rows == null || model.Rows.Count == 0)
            {
                TempData["ErrorMessage"] = "Не удалось сохранить таблицу";
                return RedirectToAction(nameof(Index));
            }

            NormalizeRows(model);

            _storage = model;

            TempData["SuccessMessage"] = "Таблица контингента сохранена";
            return RedirectToAction(nameof(Index));
        }

        private static ContingentPageViewModel CreateInitialModel()
        {
            return new ContingentPageViewModel
            {
                Rows = new List<ContingentDirectionRowViewModel>
                {
                    new()
                    {
                        DirectionCode = "09.03.04",
                        IsBachelor = true,
                        IsMaster = false,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    },
                    new()
                    {
                        DirectionCode = "09.03.03",
                        IsBachelor = true,
                        IsMaster = false,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    },
                    new()
                    {
                        DirectionCode = "09.04.04",
                        IsBachelor = false,
                        IsMaster = true,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    },
                    new()
                    {
                        DirectionCode = "09.04.03",
                        IsBachelor = false,
                        IsMaster = true,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    },
                    new()
                    {
                        DirectionCode = "09.04.03 БИ",
                        IsBachelor = false,
                        IsMaster = true,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    }
                }
            };
        }

        private static void NormalizeRows(ContingentPageViewModel model)
        {
            var defaults = CreateInitialModel().Rows;

            for (int i = 0; i < model.Rows.Count && i < defaults.Count; i++)
            {
                model.Rows[i].DirectionCode = defaults[i].DirectionCode;
                model.Rows[i].IsBachelor = defaults[i].IsBachelor;
                model.Rows[i].IsMaster = defaults[i].IsMaster;

                if (model.Rows[i].Course1Count < 0) model.Rows[i].Course1Count = 0;
                if (model.Rows[i].Course2Count < 0) model.Rows[i].Course2Count = 0;
                if (model.Rows[i].Course3Count < 0) model.Rows[i].Course3Count = 0;
                if (model.Rows[i].Course4Count < 0) model.Rows[i].Course4Count = 0;
            }
        }
    }
}