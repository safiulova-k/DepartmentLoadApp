using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.ViewModels.Workload
{
    public class WorkloadTablePageViewModel
    {
        public int SelectedYearStart { get; set; }
        public IReadOnlyList<int> AvailableYearStarts { get; set; } = Array.Empty<int>();
        public string SelectedYear { get; set; } = string.Empty;
        public List<WorkloadRow> Rows { get; set; } = new();
    }
}