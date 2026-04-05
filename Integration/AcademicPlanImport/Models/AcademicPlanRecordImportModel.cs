namespace DepartmentLoadApp.Integration.AcademicPlanImport.Models
{
    public class AcademicPlanRecordImportModel
    {
        public int Id { get; set; }

        public int AcademicPlanId { get; set; }

        public int DisciplineId { get; set; }

        public int? AcademicPlanRecordParentId { get; set; }

        public bool InDepartment { get; set; }

        public int Semester { get; set; }

        public int Zet { get; set; }

        public bool IsParent { get; set; }

        public bool IsChild { get; set; }

        public bool IsFacultative { get; set; }

        public bool IsUseInWorkload { get; set; }

        public bool IsActiveSemester { get; set; }
    }
}