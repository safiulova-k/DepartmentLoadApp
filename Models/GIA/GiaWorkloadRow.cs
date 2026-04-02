namespace DepartmentLoadApp.Models.Gia
{
    public class GiaWorkloadRow
    {
        public int Id { get; set; }

        public int PlanYear { get; set; }

        public string GiaSection { get; set; } = string.Empty;   // Госэкзамен / Дипломное проектирование / ГЭК
        public string WorkName { get; set; } = string.Empty;

        public string DirectionCode { get; set; } = string.Empty;
        public string DirectionName { get; set; } = string.Empty;

        public int Course { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public string EducationForm { get; set; } = string.Empty;

        public int StudentsCount { get; set; }
        public int GroupCount { get; set; }

        // Для консультаций к госэкзамену, где пока вручную задаём часы из JSON / UI
        public decimal ManualHours { get; set; }

        public decimal TotalHours { get; set; }
    }
}