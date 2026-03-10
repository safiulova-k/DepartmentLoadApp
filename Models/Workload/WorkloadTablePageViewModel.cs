using System.Collections.Generic;

namespace DepartmentLoadApp.Models.Workload
{
    public class WorkloadTablePageViewModel
    {
        public List<WorkloadTableRowViewModel> GuidanceRows { get; set; } = new();
        public List<WorkloadTableRowViewModel> GiaRows { get; set; } = new();
        public List<WorkloadTableRowViewModel> PracticeRows { get; set; } = new();
        public List<WorkloadTableRowViewModel> DisciplineRows { get; set; } = new();
    }
}
