namespace DepartmentLoadApp.ViewModels.WorkloadDistribution;

public class AutoDistributionResultViewModel
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; } = string.Empty;

    public string TargetAcademicYear { get; set; } = string.Empty;

    public List<string> HistoryAcademicYears { get; set; } = new();

    public int CreatedAssignmentsCount { get; set; }

    public List<AutoDistributionGroupViewModel> Groups { get; set; } = new();
}

public class AutoDistributionGroupViewModel
{
    public int LecturerId { get; set; }

    public string LecturerName { get; set; } = string.Empty;

    public string DisciplineName { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public decimal TotalHours { get; set; }

    public List<AutoDistributionAssignmentViewModel> Assignments { get; set; } = new();
}

public class AutoDistributionAssignmentViewModel
{
    public int AssignmentId { get; set; }

    public string ElementName { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public decimal Hours { get; set; }
}