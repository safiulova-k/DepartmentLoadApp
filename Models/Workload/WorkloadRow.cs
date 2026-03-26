namespace DepartmentLoadApp.Models.Workload
{
    public class WorkloadRow
    {
        public int Id { get; set; }

        public string DirectionCode { get; set; } = string.Empty;
        public string DirectionName { get; set; } = string.Empty;

        public string SemesterName { get; set; } = string.Empty;
        public string EducationForm { get; set; } = string.Empty;
        public int Course { get; set; }

        public int StudentsCount { get; set; }

        public int FlowCount { get; set; }
        public int GroupCount { get; set; }
        public int SubgroupCount { get; set; }

        public decimal LecturePlanHours { get; set; }
        public decimal PracticePlanHours { get; set; }
        public decimal LabPlanHours { get; set; }

        public decimal LectureTotalHours { get; set; }
        public decimal PracticeTotalHours { get; set; }
        public decimal LabTotalHours { get; set; }
    }
}