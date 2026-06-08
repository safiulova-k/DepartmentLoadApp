namespace DepartmentLoadApp.ViewModels.WorkloadDistribution;

public class UpdateLecturerPlanInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }

    public int LecturerId { get; set; }

    public int? LecturerStudyPostId { get; set; }

    public string Rate { get; set; } = "1";
}

public class AddSelectedAssignmentsInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }

    public int LecturerId { get; set; }

    public List<string> SelectedItemKeys { get; set; } = new();

    public List<GiaStudentsAssignmentInputModel> GiaStudents { get; set; } = new();

    public List<AdditionalWorkAssignmentInputModel> AdditionalWorks { get; set; } = new();
}

public class GiaStudentsAssignmentInputModel
{
    public string ItemKey { get; set; } = string.Empty;

    public int StudentsCount { get; set; }
}

public class AdditionalWorkAssignmentInputModel
{
    public string ItemKey { get; set; } = string.Empty;

    public int StudentsCount { get; set; }

    public decimal Hours { get; set; }
}

public class DeleteAssignmentInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }

    public int AssignmentId { get; set; }
}

public class AutoDistributeInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }
}

public class ClearDistributionInputModel
{
    public int SelectedYearStart { get; set; }

    public int? SelectedLecturerId { get; set; }
}