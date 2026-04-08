namespace DepartmentLoadApp.Models.Core;

public class EducationDirection
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public string Cipher { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Qualification { get; set; } = null!;
    public string Profile { get; set; } = null!;
    public string? Description { get; set; }
}