namespace DepartmentLoadApp.Integration.PortalMock.Models
{
    public class DisciplineImportModel
    {
        public int Id { get; set; }

        public int DisciplineBlockId { get; set; }

        public string DisciplineName { get; set; } = string.Empty;

        public string DisciplineShortName { get; set; } = string.Empty;

        public string DisciplineDescription { get; set; } = string.Empty;

        public string DisciplineBlockBlueAsteriskName { get; set; } = string.Empty;
    }
}