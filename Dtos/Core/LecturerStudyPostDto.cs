namespace DepartmentLoadApp.Dtos.Core;

public class LecturerStudyPostDto
{
    public int Id { get; set; }

    public string StudyPostTitle { get; set; } = null!;
    public int Hours { get; set; }
}