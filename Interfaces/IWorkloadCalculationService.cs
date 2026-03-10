using DepartmentLoadApp.ViewModels;

namespace DepartmentLoadApp.Interfaces
{
    public interface IWorkloadCalculationService
    {
        WorkloadCalculationPageViewModel Calculate(WorkloadCalculationPageViewModel model);
    }
}