namespace DepartmentLoadApp.ViewModels.WorkloadDistribution;

public class WorkloadDistributionSemesterGroupViewModel
{
    public string SemesterName { get; set; } = string.Empty;

    public List<WorkloadDistributionDisciplineGroupViewModel> Disciplines { get; set; } = new();
}

public class WorkloadDistributionDisciplineGroupViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public List<WorkloadDistributionWorkTypeGroupViewModel> WorkTypes { get; set; } = new();
}

public class WorkloadDistributionWorkTypeGroupViewModel
{
    public string ElementDisplayName { get; set; } = string.Empty;

    public List<WorkloadDistributionAvailableItemViewModel> Items { get; set; } = new();
}

public class WorkloadDistributionGiaItemViewModel
{
    public string ItemKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ElementDisplayName { get; set; } = string.Empty;

    public int RemainingStudentsCount { get; set; }

    public decimal HoursPerStudent { get; set; }
}