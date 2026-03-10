using System.Collections.Generic;

namespace DepartmentLoadApp.Models.Workload
{
    public class WorkloadTablePageViewModel
    {
        public List<WorkloadTableRowViewModel> DisciplineRows { get; set; } = new();
    }
}
