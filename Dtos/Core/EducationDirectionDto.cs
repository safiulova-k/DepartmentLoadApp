using DepartmentLoadApp.Models.Core.Enums;

namespace DepartmentLoadApp.Dtos.Core;

public class EducationDirectionDto
{
    public int Id { get; set; }

    public string Cipher { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string Title { get; set; } = null!;

    public EducationDirectionQualification Qualification { get; set; }

    public string Profile { get; set; } = null!;
    public string Description { get; set; } = null!;
}