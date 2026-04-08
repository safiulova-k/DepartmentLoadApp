namespace DepartmentLoadApp.Dtos.Core;

public class LecturerDto
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;

    public int LecturerStudyPostId { get; set; }
    public int LecturerDepartmentPostId { get; set; }
}