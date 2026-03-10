using DepartmentLoadApp.Interfaces;
using DepartmentLoadApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadCalculationController : Controller
    {
        private readonly IWorkloadCalculationService _workloadCalculationService;

        public WorkloadCalculationController(IWorkloadCalculationService workloadCalculationService)
        {
            _workloadCalculationService = workloadCalculationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new WorkloadCalculationPageViewModel
            {
                Rows = new List<WorkloadCalculationRowViewModel>
                {
                    new WorkloadCalculationRowViewModel()
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Calculate(WorkloadCalculationPageViewModel model)
        {
            model = _workloadCalculationService.Calculate(model);
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult AddRow(WorkloadCalculationPageViewModel model)
        {
            model.Rows ??= new List<WorkloadCalculationRowViewModel>();
            model.Rows.Add(new WorkloadCalculationRowViewModel());
            return View("Index", model);
        }
    }
}