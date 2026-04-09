using DepartmentLoadApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers;

public class IndividualPlansController : Controller
{
    private readonly IndividualPlanService _service;

    public IndividualPlansController(IndividualPlanService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? startYear)
    {
        var model = await _service.BuildPageAsync(startYear);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Export(int startYear, int lecturerId)
    {
        var result = await _service.ExportLecturerPlanAsync(startYear, lecturerId);

        if (!result.Success || result.Content == null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index), new { startYear });
        }

        return File(
            result.Content,
            result.ContentType,
            result.FileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportAll(int startYear)
    {
        var result = await _service.ExportAllPlansAsync(startYear);

        if (!result.Success || result.Content == null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index), new { startYear });
        }

        return File(
            result.Content,
            result.ContentType,
            result.FileName);
    }
}