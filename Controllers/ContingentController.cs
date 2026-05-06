using DepartmentLoadApp.Services;
using DepartmentLoadApp.ViewModels.Contingent;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers;

public class ContingentController : Controller
{
    private readonly ContingentService _contingentService;

    public ContingentController(ContingentService contingentService)
    {
        _contingentService = contingentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await _contingentService.SyncContingentAsync();

        var model = await _contingentService.BuildPageModelAsync();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubgroup(int studentGroupId)
    {
        var result = await _contingentService.AddSubgroupAsync(studentGroupId);

        return await HandleOperationResultAsync(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubgroup(int studentGroupId, int subgroupId)
    {
        var result = await _contingentService.DeleteSubgroupAsync(
            studentGroupId,
            subgroupId);

        return await HandleOperationResultAsync(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGroupSubgroups(
        int studentGroupId,
        List<int> subgroupIds,
        List<int> studentsCounts)
    {
        var result = await _contingentService.SaveGroupSubgroupsAsync(
            studentGroupId,
            subgroupIds,
            studentsCounts);

        return await HandleOperationResultAsync(result);
    }

    private async Task<IActionResult> HandleOperationResultAsync(
        ContingentOperationResult result)
    {
        if (IsAjaxRequest())
        {
            if (result.ReturnCoursePartial && result.StudentGroupId.HasValue)
            {
                return await BuildCoursePartialAsync(
                    result.StudentGroupId.Value,
                    result.IsSuccess ? null : result.Message,
                    result.ErrorStudentGroupId);
            }

            return await BuildContentPartialAsync(
                result.IsSuccess ? result.Message : null,
                result.IsSuccess ? null : result.Message,
                result.ErrorStudentGroupId);
        }

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    private async Task<IActionResult> BuildContentPartialAsync(
        string? successMessage = null,
        string? errorMessage = null,
        int? errorStudentGroupId = null)
    {
        var model = await _contingentService.BuildPageModelAsync();

        ViewData["SuccessMessage"] = successMessage;
        ViewData["ErrorMessage"] = errorMessage;
        ViewData["ErrorStudentGroupId"] = errorStudentGroupId;

        return PartialView("_ContingentContent", model);
    }

    private async Task<IActionResult> BuildCoursePartialAsync(
        int studentGroupId,
        string? errorMessage = null,
        int? errorStudentGroupId = null)
    {
        var model = await _contingentService.BuildPageModelAsync();

        var course = model.Directions
            .SelectMany(x => x.Courses)
            .First(x => x.Groups.Any(g => g.StudentGroupId == studentGroupId));

        ViewData["ErrorMessage"] = errorMessage;
        ViewData["ErrorStudentGroupId"] = errorStudentGroupId;

        return PartialView("_CourseSection", course);
    }
}