namespace DepartmentLoadApp.ViewModels.WorkloadDistribution;

public class UpdateLecturerPlanInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }

    public int LecturerId { get; set; }

    public int? LecturerStudyPostId { get; set; }

    public decimal Rate { get; set; }
}

public class AddAssignmentInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }

    public int LecturerId { get; set; }

    public string ItemKey { get; set; } = string.Empty;
}

public class ChangeAssignmentHoursInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }

    public int AssignmentId { get; set; }

    public int Delta { get; set; }
}

public class DeleteAssignmentInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }

    public int AssignmentId { get; set; }
}