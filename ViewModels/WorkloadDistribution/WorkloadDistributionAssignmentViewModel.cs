namespace DepartmentLoadApp.ViewModels.WorkloadDistribution
{
    public class WorkloadDistributionAssignmentViewModel
    {
        public int AssignmentId { get; set; }

        public string SourceTypeDisplayName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public string ElementDisplayName { get; set; } = string.Empty;

        public int AssignedHours { get; set; }

        public int TotalItemHours { get; set; }

        public int RemainingItemHours { get; set; }

        public bool CanIncrease { get; set; }

        public bool CanDecrease { get; set; }
    }

}
