using DepartmentLoadApp.Models.Core;
using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Models;

public class LecturerLoadAssignment
{
    public int Id { get; set; }

    public string AcademicYear { get; set; } = string.Empty;

    public int LecturerAcademicYearPlanId { get; set; }
    public LecturerAcademicYearPlan? LecturerAcademicYearPlan { get; set; }

    public LoadAssignmentSourceType SourceType { get; set; }

    public int SourceRowId { get; set; }

    public LoadAssignmentElementType LoadElementType { get; set; }

    public int AssignedHours { get; set; }
}