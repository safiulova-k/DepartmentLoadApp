using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class AcademicYearsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
