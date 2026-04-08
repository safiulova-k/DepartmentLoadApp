namespace DepartmentLoadApp.Models.Core;

public class LecturerStudyPost
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public string Title { get; set; } = null!;
    public double Hours { get; set; }
}