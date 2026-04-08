namespace DepartmentLoadApp.Models.Core;

public class AcademicPlanRecord
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public int AcademicPlanId { get; set; }

    public string Index { get; set; } = null!;
    public string Name { get; set; } = null!;

    public int Semester { get; set; }
    public int Zet { get; set; }
    public int AcademicHours { get; set; }

    public int? Exam { get; set; }
    public int? Pass { get; set; }
    public int? GradedPass { get; set; }
    public int? CourseWork { get; set; }
    public int? CourseProject { get; set; }
    public int? Rgr { get; set; }

    public int? Lectures { get; set; }
    public int? LaboratoryHours { get; set; }
    public int? PracticalHours { get; set; }
}