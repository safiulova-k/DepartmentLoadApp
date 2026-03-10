using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class IndividualPlansController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
