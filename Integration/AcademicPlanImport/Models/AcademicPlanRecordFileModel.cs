namespace DepartmentLoadApp.Integration.AcademicPlanImport.Models
{
    public class AcademicPlanRecordFileModel
    {
        public List<PlanRecordModel> PlanRecords { get; set; } = new();
    }

    public class PlanRecordModel
    {
        public int AcademicPlanId { get; set; }
        public int DisciplineId { get; set; }
        public int Semester { get; set; }
        public decimal Zet { get; set; }
        public bool IsActiveSemester { get; set; }
        public int DisciplineBlockId { get; set; }
    }
}