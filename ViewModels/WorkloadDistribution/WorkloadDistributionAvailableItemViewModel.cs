namespace DepartmentLoadApp.ViewModels.WorkloadDistribution
{
    public class WorkloadDistributionAvailableItemViewModel
    {
        public string ItemKey { get; set; } = string.Empty;

        public string SourceTypeDisplayName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public string ElementDisplayName { get; set; } = string.Empty;

        public int TotalHours { get; set; }

        public int AssignedHours { get; set; }

        public int RemainingHours { get; set; }

        public string SelectText =>
            $"{Title} — {ElementDisplayName} ({RemainingHours} ч. осталось)";
    }
}
