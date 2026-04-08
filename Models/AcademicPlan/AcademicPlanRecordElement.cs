using DepartmentLoadApp.Models.Core;

namespace DepartmentLoadApp.Models.AcademicPlan
{
    public class AcademicPlanRecordElement
    {
        public int Id { get; set; }

        public int AcademicPlanRecordId { get; set; }

        public string ActivityType { get; set; } = string.Empty;

        public decimal PlanHours { get; set; }

        public decimal FactHours { get; set; }

        public AcademicPlanRecord? AcademicPlanRecord { get; set; }
    }
}