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

    // Старый временный Id строки расчета.
    // Оставляем, чтобы не ломать старые данные и миграцию.
    public int SourceRowId { get; set; }

    // Новый стабильный ключ из AcademicPlanRecord
    public int SourceAcademicPlanRecordId { get; set; }

    public LoadAssignmentElementType LoadElementType { get; set; }

    public int AssignedHours { get; set; }
}