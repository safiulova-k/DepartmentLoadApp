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

    public int SourceAcademicPlanRecordId { get; set; }

    public LoadAssignmentElementType LoadElementType { get; set; }

    public DistributionUnitType DistributionUnitType { get; set; }

    public int? StudentGroupId { get; set; }

    public int? ContingentSubgroupId { get; set; }

    public string UnitName { get; set; } = string.Empty;

    public int StudentsCount { get; set; }

    public decimal AssignedHours { get; set; }
}