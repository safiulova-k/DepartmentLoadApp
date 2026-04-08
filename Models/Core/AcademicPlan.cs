using DepartmentLoadApp.Models.Core.Enums;
using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Models.Core;

public class AcademicPlan
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public int? EducationDirectionId { get; set; }

    public EducationForm EducationForm { get; set; }
    public AcademicCourse AcademicCourses { get; set; }

    public string Year { get; set; } = null!;
}