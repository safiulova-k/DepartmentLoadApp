namespace DepartmentLoadApp.Models.Practice
{
    public class PracticeWorkloadRow
    {
        public int Id { get; set; }

        public int PlanYear { get; set; }

        public string PracticeName { get; set; } = string.Empty;

        public string DirectionCode { get; set; } = string.Empty;

        public string DirectionName { get; set; } = string.Empty;

        public int Course { get; set; }

        public int StudentsCount { get; set; }

        public decimal WeeksCount { get; set; }

        public decimal TotalHours { get; set; }
    }
}