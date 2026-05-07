namespace DepartmentLoadApp.ViewModels.WorkloadDistribution;

public class WorkloadDistributionAvailableItemViewModel
{
    public string ItemKey { get; set; } = string.Empty;

    public string SourceTypeDisplayName { get; set; } = string.Empty;

    public string SemesterName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ElementDisplayName { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public decimal TotalHours { get; set; }

    public decimal AssignedHours { get; set; }

    public decimal RemainingHours { get; set; }

    public int StudentsCount { get; set; }

    public bool IsGiaStudentsInput { get; set; }

    public int RemainingStudentsCount { get; set; }

    public decimal HoursPerStudent { get; set; }
}