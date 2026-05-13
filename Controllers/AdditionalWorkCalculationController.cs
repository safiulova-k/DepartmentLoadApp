using DepartmentLoadApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class AdditionalWorkCalculationController : Controller
    {
        private readonly AdditionalWorkCalculationService _additionalWorkCalculationService;

        public AdditionalWorkCalculationController(
            AdditionalWorkCalculationService additionalWorkCalculationService)
        {
            _additionalWorkCalculationService = additionalWorkCalculationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? startYear)
        {
            var model = await _additionalWorkCalculationService.BuildPageAsync(startYear);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePostgraduateCount(
            int selectedYearStart,
            int count)
        {
            await _additionalWorkCalculationService.SavePostgraduateCountAsync(
                selectedYearStart,
                count);

            TempData["SuccessMessage"] = "Расчет дополнительной работы сохранен.";

            return RedirectToAction(nameof(Index), new
            {
                startYear = selectedYearStart
            });
        }
    }
}