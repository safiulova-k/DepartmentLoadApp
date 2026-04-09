namespace DepartmentLoadApp.ViewModels.IndividualPlans;

public class IndividualPlanLecturerRowViewModel
{
    public int LecturerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudyPostTitle { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public int AssignedHours { get; set; }
}