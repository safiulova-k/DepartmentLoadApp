using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly IEducationDirectionSyncService _educationDirectionSyncService;
    private readonly ILecturerStudyPostSyncService _lecturerStudyPostSyncService;
    private readonly ILecturerDepartmentPostSyncService _lecturerDepartmentPostSyncService;
    private readonly ILecturerSyncService _lecturerSyncService;
    private readonly IStudentGroupSyncService _studentGroupSyncService;
    private readonly IAcademicPlanSyncService _academicPlanSyncService;
    private readonly IAcademicPlanRecordSyncService _academicPlanRecordSyncService;

    public SyncController(
        IEducationDirectionSyncService educationDirectionSyncService,
        ILecturerStudyPostSyncService lecturerStudyPostSyncService,
        ILecturerDepartmentPostSyncService lecturerDepartmentPostSyncService,
        ILecturerSyncService lecturerSyncService,
        IStudentGroupSyncService studentGroupSyncService,
        IAcademicPlanSyncService academicPlanSyncService,
        IAcademicPlanRecordSyncService academicPlanRecordSyncService)
    {
        _educationDirectionSyncService = educationDirectionSyncService;
        _lecturerStudyPostSyncService = lecturerStudyPostSyncService;
        _lecturerDepartmentPostSyncService = lecturerDepartmentPostSyncService;
        _lecturerSyncService = lecturerSyncService;
        _studentGroupSyncService = studentGroupSyncService;
        _academicPlanSyncService = academicPlanSyncService;
        _academicPlanRecordSyncService = academicPlanRecordSyncService;
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
            await _educationDirectionSyncService.Sync();
            return Ok("EducationDirections synced");
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
            await _lecturerStudyPostSyncService.Sync();
            return Ok("LecturerStudyPosts synced");
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
            await _lecturerDepartmentPostSyncService.Sync();
            return Ok("LecturerDepartmentPosts synced");
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
            await _lecturerSyncService.Sync();
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
            await _studentGroupSyncService.Sync();
            return Ok("StudentGroups synced");
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
            await _academicPlanSyncService.Sync();
            return Ok("AcademicPlans synced");
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
            await _academicPlanRecordSyncService.Sync();
            return Ok("AcademicPlanRecords synced");
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
            await _educationDirectionSyncService.Sync();
            await _lecturerStudyPostSyncService.Sync();
            await _lecturerDepartmentPostSyncService.Sync();
            await _lecturerSyncService.Sync();
            await _studentGroupSyncService.Sync();
            await _academicPlanSyncService.Sync();
            await _academicPlanRecordSyncService.Sync();

            return Ok("Core sync completed");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }
}