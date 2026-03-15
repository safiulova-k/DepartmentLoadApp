using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models.Contingent;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class ContingentController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(AppMemoryStore.Contingent);
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
            AppMemoryStore.Contingent = model;

            TempData["SuccessMessage"] = "Таблица контингента сохранена";
            return RedirectToAction(nameof(Index));
        }

        private static void NormalizeRows(ContingentPageViewModel model)
        {
            var defaults = AppMemoryStore.Contingent.Rows;

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