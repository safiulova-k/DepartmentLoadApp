namespace DepartmentLoadApp.Models.Core;

public class Lecturer
{
    public int Id { get; set; }
    public int CoreId { get; set; }

    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;

    public int LecturerStudyPostId { get; set; }
    public int LecturerDepartmentPostId { get; set; }

    public LecturerStudyPost? StudyPost { get; set; }
    public LecturerDepartmentPost? DepartmentPost { get; set; }
}