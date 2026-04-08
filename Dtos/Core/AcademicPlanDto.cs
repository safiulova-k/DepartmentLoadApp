using DepartmentLoadApp.Models.Core.Enums;

namespace DepartmentLoadApp.Dtos.Core;

public class AcademicPlanDto
{
    public int Id { get; set; }

    public int? EducationDirectionId { get; set; }

    public EducationForm EducationForm { get; set; }
    public AcademicCourse AcademicCourses { get; set; }

    public string Year { get; set; } = null!;
}