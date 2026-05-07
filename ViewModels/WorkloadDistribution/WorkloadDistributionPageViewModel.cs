namespace DepartmentLoadApp.ViewModels.WorkloadDistribution;

public class WorkloadDistributionPageViewModel
{
    public int SelectedYearStart { get; set; }

    public string SelectedYear { get; set; } = string.Empty;

    public IReadOnlyList<int> AvailableYearStarts { get; set; } = Array.Empty<int>();

    public int? SelectedLecturerId { get; set; }

    public decimal TotalHours { get; set; }

    public decimal AssignedHours { get; set; }

    public decimal RemainingHours { get; set; }

    public int OverloadedLecturerCount { get; set; }

    public List<WorkloadDistributionStudyPostOptionViewModel> StudyPosts { get; set; } = new();

    public List<WorkloadDistributionLecturerCardViewModel> Lecturers { get; set; } = new();

    public List<WorkloadDistributionAvailableItemViewModel> RemainingItems { get; set; } = new();
}