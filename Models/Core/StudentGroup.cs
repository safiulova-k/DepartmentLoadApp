namespace DepartmentLoadApp.Models.Core;

public class StudentGroup
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public int EducationDirectionId { get; set; }
    public int? CuratorId { get; set; }

    public string GroupName { get; set; } = null!;
    public int Course { get; set; }

    public EducationDirection? EducationDirection { get; set; }
    public Lecturer? Curator { get; set; }
}