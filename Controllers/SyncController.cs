using DepartmentLoadApp.Integration.CoreSync;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers;

[ApiController]
[Route("sync")]
public class SyncController : ControllerBase
{
    private readonly EducationDirectionSyncService _education;
    private readonly LecturerStudyPostSyncService _studyPost;
    private readonly LecturerDepartmentPostSyncService _depPost;
    private readonly LecturerSyncService _lecturer;
    private readonly StudentGroupSyncService _group;
    private readonly AcademicPlanSyncService _plan;
    private readonly AcademicPlanRecordSyncService _record;

    public SyncController(
        EducationDirectionSyncService education,
        LecturerStudyPostSyncService studyPost,
        LecturerDepartmentPostSyncService depPost,
        LecturerSyncService lecturer,
        StudentGroupSyncService group,
        AcademicPlanSyncService plan,
        AcademicPlanRecordSyncService record)
    {
        _education = education;
        _studyPost = studyPost;
        _depPost = depPost;
        _lecturer = lecturer;
        _group = group;
        _plan = plan;
        _record = record;
    }

    [HttpPost("all")]
    public async Task<IActionResult> SyncAll()
    {
        await _education.Sync();
        await _studyPost.Sync();
        await _depPost.Sync();
        await _lecturer.Sync();
        await _group.Sync();
        await _plan.Sync();
        await _record.Sync();

        return Ok("Sync completed");
    }
}