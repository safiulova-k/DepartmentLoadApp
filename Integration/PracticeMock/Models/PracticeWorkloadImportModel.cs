namespace DepartmentLoadApp.Integration.PracticeMock.Models
{
    public class PracticeWorkloadImportModel
    {
        public int PlanYear { get; set; }

        public string PracticeName { get; set; } = string.Empty;

        public string DirectionCode { get; set; } = string.Empty;

        public string DirectionName { get; set; } = string.Empty;

        public int Course { get; set; }

        public string SemesterName { get; set; } = string.Empty;

        public string EducationForm { get; set; } = string.Empty;

        public int WeeksCount { get; set; }
    }
}