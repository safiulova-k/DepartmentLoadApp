using System.Collections.Generic;
using System.Linq;

namespace DepartmentLoadApp.Models.Workload
{
    public class WorkloadTablePageViewModel
    {
        public List<WorkloadTableRowViewModel> Rows { get; set; } = new();

        public decimal TotalLectureHours => Rows.Sum(x => x.LectureTotalHours);
        public decimal TotalPracticeHours => Rows.Sum(x => x.PracticeTotalHours);
        public decimal TotalLabHours => Rows.Sum(x => x.LabTotalHours);
        public decimal GrandTotalHours => Rows.Sum(x => x.TotalHours);
    }
}