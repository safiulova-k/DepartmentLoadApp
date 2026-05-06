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
        return await RunSyncAsync(
            _educationDirectionSyncService.Sync,
            "Направления подготовки синхронизированы");
    }

    [HttpPost("lecturer-study-posts")]
    public async Task<IActionResult> SyncLecturerStudyPosts()
    {
        return await RunSyncAsync(
            _lecturerStudyPostSyncService.Sync,
            "Учебные должности преподавателей синхронизированы");
    }

    [HttpPost("lecturer-department-posts")]
    public async Task<IActionResult> SyncLecturerDepartmentPosts()
    {
        return await RunSyncAsync(
            _lecturerDepartmentPostSyncService.Sync,
            "Кафедральные должности преподавателей синхронизированы");
    }

    [HttpPost("lecturers")]
    public async Task<IActionResult> SyncLecturers()
    {
        return await RunSyncAsync(
            _lecturerSyncService.Sync,
            "Преподаватели синхронизированы");
    }

    [HttpPost("student-groups")]
    public async Task<IActionResult> SyncStudentGroups()
    {
        return await RunSyncAsync(
            _studentGroupSyncService.Sync,
            "Студенческие группы синхронизированы");
    }

    [HttpPost("academic-plans")]
    public async Task<IActionResult> SyncAcademicPlans()
    {
        return await RunSyncAsync(
            _academicPlanSyncService.Sync,
            "Учебные планы синхронизированы");
    }

    [HttpPost("academic-plan-records")]
    public async Task<IActionResult> SyncAcademicPlanRecords()
    {
        return await RunSyncAsync(
            _academicPlanRecordSyncService.Sync,
            "Записи учебных планов синхронизированы");
    }

    [HttpPost("all")]
    public async Task<IActionResult> SyncAll()
    {
        return await RunSyncAsync(async () =>
        {
            await _educationDirectionSyncService.Sync();
            await _lecturerStudyPostSyncService.Sync();
            await _lecturerDepartmentPostSyncService.Sync();
            await _lecturerSyncService.Sync();
            await _studentGroupSyncService.Sync();
            await _academicPlanSyncService.Sync();
            await _academicPlanRecordSyncService.Sync();
        }, "Синхронизация всех данных завершена");
    }

    private async Task<IActionResult> RunSyncAsync(Func<Task> syncAction, string successMessage)
    {
        try
        {
            await syncAction();

            return Ok(successMessage);
        }
        catch
        {
            return StatusCode(500, "Ошибка при синхронизации данных");
        }
    }
}