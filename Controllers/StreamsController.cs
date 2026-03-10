using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class StreamsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
