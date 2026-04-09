namespace DepartmentLoadApp.ViewModels.IndividualPlans;

public class IndividualPlansPageViewModel
{
    public int SelectedYearStart { get; set; }
    public string SelectedAcademicYear { get; set; } = string.Empty;
    public IReadOnlyList<int> AvailableYearStarts { get; set; } = Array.Empty<int>();
    public bool TemplateExists { get; set; }

    public List<IndividualPlanLecturerRowViewModel> Lecturers { get; set; } = new();
}