namespace DepartmentLoadApp.Integration.PortalMock.Models
{
    public class AcademicPlanRecordElementImportModel
    {
        public int Id { get; set; }

        public int AcademicPlanRecordId { get; set; }

        public string ActivityType { get; set; } = string.Empty;

        public decimal PlanHours { get; set; }

        public decimal FactHours { get; set; }
    }
}