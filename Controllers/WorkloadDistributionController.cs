using DepartmentLoadApp.Services;
using DepartmentLoadApp.ViewModels.WorkloadDistribution;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers;

public class WorkloadDistributionController : Controller
{
    private readonly WorkloadDistributionService _service;

    public WorkloadDistributionController(WorkloadDistributionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? startYear, int? selectedLecturerId)
    {
        var model = await _service.BuildPageAsync(startYear, selectedLecturerId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FillAssignmentToRemaining(int startYear, int assignmentId)
    {
        var result = await _service.FillAssignmentToMaxAsync(startYear, assignmentId);

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index), new
        {
            startYear,
            selectedLecturerId = result.LecturerId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLecturerPlan(UpdateLecturerPlanInputModel model)
    {
        var result = await _service.SaveLecturerPlanAsync(
            model.SelectedYearStart,
            model.LecturerId,
            model.LecturerStudyPostId,
            model.Rate);

        PutMessage(result);

        return RedirectToAction(nameof(Index), new
        {
            startYear = model.SelectedYearStart,
            selectedLecturerId = result.LecturerId ?? model.SelectedLecturerId ?? model.LecturerId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAssignment(AddAssignmentInputModel model)
    {
        var result = await _service.AddAssignmentAsync(
            model.SelectedYearStart,
            model.LecturerId,
            model.ItemKey);

        PutMessage(result);

        return RedirectToAction(nameof(Index), new
        {
            startYear = model.SelectedYearStart,
            selectedLecturerId = result.LecturerId ?? model.SelectedLecturerId ?? model.LecturerId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeAssignmentHours(ChangeAssignmentHoursInputModel model)
    {
        var result = await _service.ChangeAssignmentHoursAsync(
            model.SelectedYearStart,
            model.AssignmentId,
            model.Delta);

        PutMessage(result);

        return RedirectToAction(nameof(Index), new
        {
            startYear = model.SelectedYearStart,
            selectedLecturerId = result.LecturerId ?? model.SelectedLecturerId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAssignment(DeleteAssignmentInputModel model)
    {
        var result = await _service.DeleteAssignmentAsync(
            model.SelectedYearStart,
            model.AssignmentId);

        PutMessage(result);

        return RedirectToAction(nameof(Index), new
        {
            startYear = model.SelectedYearStart,
            selectedLecturerId = result.LecturerId ?? model.SelectedLecturerId
        });
    }

    private void PutMessage(WorkloadDistributionOperationResult result)
    {
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }
    }
}