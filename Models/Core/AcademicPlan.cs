using DepartmentLoadApp.Models.AcademicPlan;

namespace DepartmentLoadApp.Models.Core;

public class AcademicPlan
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public int EducationDirectionId { get; set; }

    public string EducationForm { get; set; } = null!;
    public int AcademicCourses { get; set; }
    public int Year { get; set; }

    public EducationDirection? EducationDirection { get; set; }
    public ICollection<AcademicPlanRecord>? Records { get; set; }
}