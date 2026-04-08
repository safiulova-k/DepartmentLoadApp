namespace DepartmentLoadApp.Dtos.Core;

public class LecturerDepartmentPostDto
{
    public int Id { get; set; }

    public string DepartmentPostTitle { get; set; } = null!;
    public int Order { get; set; }
}