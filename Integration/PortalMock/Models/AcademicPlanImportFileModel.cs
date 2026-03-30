namespace DepartmentLoadApp.Integration.PortalMock.Models
{
    public class AcademicPlanImportFileModel
    {
        public List<AcademicPlanImportModel> AcademicPlans { get; set; } = new();

        public List<AcademicPlanRecordImportModel> AcademicPlanRecords { get; set; } = new();

        public List<DisciplineImportModel> Disciplines { get; set; } = new();

        public List<AcademicPlanRecordElementImportModel> AcademicPlanRecordElements { get; set; } = new();
    }
}