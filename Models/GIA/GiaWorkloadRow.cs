namespace DepartmentLoadApp.Models.Gia
{
    public class GiaWorkloadRow
    {
        public int Id { get; set; }

        public string PlanYear { get; set; } = string.Empty;

        public int AcademicPlanId { get; set; }
        public int AcademicPlanRecordId { get; set; }

        public string GiaSection { get; set; } = string.Empty;
        public string WorkName { get; set; } = string.Empty;

        public string DirectionCode { get; set; } = string.Empty;
        public string DirectionName { get; set; } = string.Empty;

        public int Course { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public string EducationForm { get; set; } = string.Empty;

        public int StudentsCount { get; set; }
        public int GroupCount { get; set; }

        public decimal ManualHours { get; set; }
        public decimal TotalHours { get; set; }
    }
}