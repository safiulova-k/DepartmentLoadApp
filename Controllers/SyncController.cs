using DepartmentLoadApp.Integration.CoreSync;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers;

[ApiController]
[Route("sync")]
public class SyncController : ControllerBase
{
    private readonly EducationDirectionSyncService _education;
    private readonly LecturerStudyPostSyncService _studyPost;
    private readonly LecturerDepartmentPostSyncService _departmentPost;
    private readonly LecturerSyncService _lecturer;
    private readonly StudentGroupSyncService _studentGroup;
    private readonly AcademicPlanSyncService _academicPlan;
    private readonly AcademicPlanRecordSyncService _academicPlanRecord;

    public SyncController(
        EducationDirectionSyncService education,
        LecturerStudyPostSyncService studyPost,
        LecturerDepartmentPostSyncService departmentPost,
        LecturerSyncService lecturer,
        StudentGroupSyncService studentGroup,
        AcademicPlanSyncService academicPlan,
        AcademicPlanRecordSyncService academicPlanRecord)
    {
        _education = education;
        _studyPost = studyPost;
        _departmentPost = departmentPost;
        _lecturer = lecturer;
        _studentGroup = studentGroup;
        _academicPlan = academicPlan;
        _academicPlanRecord = academicPlanRecord;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("Sync controller works");
    }

    [HttpPost("education-directions")]
    public async Task<IActionResult> SyncEducationDirections()
    {
        try
        {
            await _education.Sync();
            return Ok("Education directions synced");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("lecturer-study-posts")]
    public async Task<IActionResult> SyncLecturerStudyPosts()
    {
        try
        {
            await _studyPost.Sync();
            return Ok("Lecturer study posts synced");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("lecturer-department-posts")]
    public async Task<IActionResult> SyncLecturerDepartmentPosts()
    {
        try
        {
            await _departmentPost.Sync();
            return Ok("Lecturer department posts synced");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("lecturers")]
    public async Task<IActionResult> SyncLecturers()
    {
        try
        {
            await _lecturer.Sync();
            return Ok("Lecturers synced");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("student-groups")]
    public async Task<IActionResult> SyncStudentGroups()
    {
        try
        {
            await _studentGroup.Sync();
            return Ok("Student groups synced");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("academic-plans")]
    public async Task<IActionResult> SyncAcademicPlans()
    {
        try
        {
            await _academicPlan.Sync();
            return Ok("Academic plans synced");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("academic-plan-records")]
    public async Task<IActionResult> SyncAcademicPlanRecords()
    {
        try
        {
            await _academicPlanRecord.Sync();
            return Ok("Academic plan records synced");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpPost("all")]
    public async Task<IActionResult> SyncAll()
    {
        try
        {
            await _education.Sync();
            await _studyPost.Sync();
            await _departmentPost.Sync();
            await _lecturer.Sync();
            await _studentGroup.Sync();
            await _academicPlan.Sync();
            await _academicPlanRecord.Sync();

            return Ok("Sync completed");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }
}