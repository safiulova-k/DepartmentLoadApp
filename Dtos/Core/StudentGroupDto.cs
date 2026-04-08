using DepartmentLoadApp.Models.Core.Enums;

namespace DepartmentLoadApp.Dtos.Core;

public class StudentGroupDto
{
    public int Id { get; set; }

    public int EducationDirectionId { get; set; }
    public int? CuratorId { get; set; }

    public string GroupName { get; set; } = null!;
    public AcademicCourse Course { get; set; }
}