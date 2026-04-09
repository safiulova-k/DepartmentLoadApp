namespace DepartmentLoadApp.Models.Core;

public class LecturerAcademicYearPlan
{
    public int Id { get; set; }

    public string AcademicYear { get; set; } = string.Empty;

    public int LecturerId { get; set; }
    public Lecturer? Lecturer { get; set; }

    public int? LecturerStudyPostId { get; set; }
    public LecturerStudyPost? LecturerStudyPost { get; set; }

    public decimal Rate { get; set; } = 1.00m;
}