using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.ViewModels.Workload
{
    public class WorkloadTablePageViewModel
    {
        public string SelectedYear { get; set; } = string.Empty;
        public List<WorkloadRow> Rows { get; set; } = new();
    }
}