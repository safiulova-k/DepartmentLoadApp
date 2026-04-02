namespace DepartmentLoadApp.Integration.GiaMock.Models
{
    public class GiaWorkloadImportRowModel
    {
        public int PlanYear { get; set; }

        public string DirectionCode { get; set; } = string.Empty;
        public string DirectionName { get; set; } = string.Empty;

        public int Course { get; set; }

        public int Semester { get; set; }

        public string EducationForm { get; set; } = string.Empty;

        public bool HasStateExam { get; set; }
        public bool HasVkr { get; set; }

        public decimal StateExamConsultationHours { get; set; }
    }
}