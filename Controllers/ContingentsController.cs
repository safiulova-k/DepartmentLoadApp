using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class ContingentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
