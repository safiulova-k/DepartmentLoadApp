using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("DepartmentLoadApp is running");
    }
}