using DepartmentLoadApp.Models.Core.Enums;

namespace DepartmentLoadApp.Dtos.Core;

public class LecturerDto
{
    public int Id { get; set; }

    public int? LecturerStudyPostId { get; set; }
    public int LecturerDepartmentPostId { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Patronymic { get; set; } = null!;
    public string Abbreviation { get; set; } = null!;

    public DateTime DateBirth { get; set; }

    public string Address { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string HomeNumber { get; set; } = null!;

    public Rank Rank { get; set; }
    public Rank2 Rank2 { get; set; }

    public string Description { get; set; } = null!;
    public byte[]? Photo { get; set; }

    public bool OnlyForPrivate { get; set; }
}