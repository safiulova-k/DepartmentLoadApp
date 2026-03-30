using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.ViewModels.Workload
{
    public class WorkloadTablePageViewModel
    {
        public int SelectedYear { get; set; }

        public List<WorkloadRow> Rows { get; set; } = new();
    }
}