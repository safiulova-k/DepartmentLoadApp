namespace DepartmentLoadApp.ViewModels.WorkloadDistribution
{
    public class WorkloadDistributionLecturerCardViewModel
    {
        public int LecturerId { get; set; }

        public string LecturerDisplayName { get; set; } = string.Empty;

        public int? LecturerStudyPostId { get; set; }

        public string LecturerStudyPostTitle { get; set; } = "Не выбрана";

        public decimal Rate { get; set; }

        public int NormHours { get; set; }

        public int LimitHours { get; set; }

        public int AssignedHours { get; set; }

        public int RemainingHours { get; set; }

        public bool IsAssistant { get; set; }

        public bool IsOverloaded { get; set; }

        public List<WorkloadDistributionAvailableItemViewModel> AvailableItems { get; set; } = new();

        public List<WorkloadDistributionAssignmentViewModel> Assignments { get; set; } = new();
    }
}
