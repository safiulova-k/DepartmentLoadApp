using DepartmentLoadApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadDistributionController : Controller
    {
        public IActionResult Index()
        {
            var model = new List<WorkloadDistributionRowViewModel>
            {
                new WorkloadDistributionRowViewModel
                {
                    DisciplineName = "Базы данных",
                    GroupName = "ПИбд-31",
                    TotalHours = 101,
                    LecturerName = "Иванов И.И.",
                    AssignedHours = 50
                },
                new WorkloadDistributionRowViewModel
                {
                    DisciplineName = "Программная инженерия",
                    GroupName = "ПИбд-32",
                    TotalHours = 94,
                    LecturerName = "Петров П.П.",
                    AssignedHours = 60
                }
            };

            return View(model);
        }
    }
}