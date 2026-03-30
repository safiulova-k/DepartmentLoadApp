using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.ViewModels.Workload
{
    public class WorkloadTablePageViewModel
    {
        public int SelectedYear { get; set; }

        public List<WorkloadRow> Rows { get; set; } = new();

        public decimal TotalLectureHours => Rows.Sum(x => x.LectureTotalHours);

        public decimal TotalPracticeHours => Rows.Sum(x => x.PracticeTotalHours);

        public decimal TotalLabHours => Rows.Sum(x => x.LabTotalHours);

        public decimal GrandTotalHours => TotalLectureHours + TotalPracticeHours + TotalLabHours;
    }
}