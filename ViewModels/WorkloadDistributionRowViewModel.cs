namespace DepartmentLoadApp.ViewModels
{
    public class WorkloadDistributionRowViewModel
    {
        public string DisciplineName { get; set; } = string.Empty;

        public string GroupName { get; set; } = string.Empty;

        public decimal TotalHours { get; set; }

        public string LecturerName { get; set; } = string.Empty;

        public decimal AssignedHours { get; set; }
    }
}