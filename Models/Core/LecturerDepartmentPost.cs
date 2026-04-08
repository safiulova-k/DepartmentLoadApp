namespace DepartmentLoadApp.Models.Core;

public class LecturerDepartmentPost
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public string Title { get; set; } = null!;
    public int Order { get; set; }
}