namespace DepartmentLoadApp.Models.Core;

public class LecturerStudyPost
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public string StudyPostTitle { get; set; } = null!;
    public int Hours { get; set; }
}