namespace DepartmentLoadApp.Dtos.Core;

public class AcademicPlanDto
{
    public int Id { get; set; }

    public int EducationDirectionId { get; set; }

    public string EducationForm { get; set; } = null!;
    public int AcademicCourses { get; set; }
    public int Year { get; set; }
}