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
    }
}