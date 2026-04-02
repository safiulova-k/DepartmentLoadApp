using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Models.AcademicPlan
{
    public class AcademicPlanRecord
    {
        public int Id { get; set; }

        public int AcademicPlanId { get; set; }

        public int DisciplineId { get; set; }

        public int? AcademicPlanRecordParentId { get; set; }

        public bool InDepartment { get; set; }

        public Semester Semester { get; set; }

        public int Zet { get; set; }

        public bool IsParent { get; set; }

        public bool IsChild { get; set; }

        public bool IsFacultative { get; set; }

        public bool IsUseInWorkload { get; set; }

        public bool IsActiveSemester { get; set; }

        public AcademicPlan? AcademicPlan { get; set; }

        public Discipline? Discipline { get; set; }

        public AcademicPlanRecord? ParentRecord { get; set; }

        public List<AcademicPlanRecord> ChildRecords { get; set; } = new();

        public List<AcademicPlanRecordElement> Elements { get; set; } = new();
        public int DisciplineBlockId { get; set; }
    }
}