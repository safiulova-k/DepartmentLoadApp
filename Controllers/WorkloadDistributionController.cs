using System.Globalization;
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
    public async Task<IActionResult> SaveLecturerPlan(UpdateLecturerPlanInputModel model)
    {
        var rate = ParseRate(model.Rate);

        var result = await _service.SaveLecturerPlanAsync(
            model.SelectedYearStart,
            model.LecturerId,
            model.LecturerStudyPostId,
            rate);

        PutMessage(result);

        return RedirectToIndex(
            model.SelectedYearStart,
            result.LecturerId ?? model.SelectedLecturerId ?? model.LecturerId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSelectedAssignments(AddSelectedAssignmentsInputModel model)
    {
        var result = await _service.AddSelectedAssignmentsAsync(
          model.SelectedYearStart,
          model.LecturerId,
          model.SelectedItemKeys,
          model.GiaStudents,
          model.AdditionalWorks);

        PutMessage(result);

        return RedirectToIndex(
            model.SelectedYearStart,
            result.LecturerId ?? model.SelectedLecturerId ?? model.LecturerId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAssignment(DeleteAssignmentInputModel model)
    {
        var result = await _service.DeleteAssignmentAsync(
            model.SelectedYearStart,
            model.AssignmentId);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                lecturerId = result.LecturerId ?? model.SelectedLecturerId
            });
        }

        PutMessage(result);

        return RedirectToIndex(
            model.SelectedYearStart,
            result.LecturerId ?? model.SelectedLecturerId);
    }

    private RedirectToActionResult RedirectToIndex(
        int startYear,
        int? selectedLecturerId)
    {
        return RedirectToAction(nameof(Index), new { startYear, selectedLecturerId });
    }

    private void PutMessage(WorkloadDistributionOperationResult result)
    {
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
    }

    private static decimal ParseRate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 1m;
        }

        var normalized = value.Trim().Replace(',', '.');

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var rate)
            ? rate
            : 1m;
    }
}