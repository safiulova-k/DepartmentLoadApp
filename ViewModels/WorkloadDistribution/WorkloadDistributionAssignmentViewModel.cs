namespace DepartmentLoadApp.ViewModels.WorkloadDistribution;

public class WorkloadDistributionAssignmentViewModel
{
    public int AssignmentId { get; set; }

    public string SourceTypeDisplayName { get; set; } = string.Empty;

    public string SemesterName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ElementDisplayName { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public int StudentsCount { get; set; }

    public decimal AssignedHours { get; set; }

    public decimal TotalItemHours { get; set; }

    public decimal RemainingItemHours { get; set; }
}