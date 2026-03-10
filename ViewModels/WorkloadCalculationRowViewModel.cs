namespace DepartmentLoadApp.ViewModels
{
    public class WorkloadCalculationRowViewModel
    {
        public string Semester { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string EducationForm { get; set; } = string.Empty;

        public string EducationLevel { get; set; } = string.Empty;

        public string DirectionCode { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public string ActivityType { get; set; } = string.Empty;

        public string DisciplineName { get; set; } = string.Empty;

        public int Course { get; set; }

        public int StudentCount { get; set; }

        public int StreamCount { get; set; }

        public int GroupCount { get; set; }

        public int SubGroupCount { get; set; }

        public decimal LecturePlanHours { get; set; }

        public decimal LectureTotalHours { get; set; }

        public decimal PracticePlanHours { get; set; }

        public decimal PracticeTotalHours { get; set; }

        public decimal LaboratoryPlanHours { get; set; }

        public decimal LaboratoryTotalHours { get; set; }

        public bool HasConsultation { get; set; }

        public bool HasExam { get; set; }

        public bool HasCredit { get; set; }

        public bool HasCourseWork { get; set; }

        public bool HasCourseProject { get; set; }

        public bool HasReferatRgr { get; set; }

        public bool HasDiploma { get; set; }

        public bool HasGek { get; set; }

        public bool HasPracticeManagement { get; set; }

        public bool HasEducationalPractice { get; set; }

        public bool HasIndustrialPractice { get; set; }

        public bool HasPreDiplomaPractice { get; set; }

        public bool HasScientificPedagogicalPractice { get; set; }

        public bool HasScientificQualificationWork { get; set; }

        public bool HasParticipationInGek { get; set; }

        public decimal? ConsultationHours { get; set; }

        public decimal? ExamHours { get; set; }

        public decimal? CreditHours { get; set; }

        public decimal? CourseWorkHours { get; set; }

        public decimal? CourseProjectHours { get; set; }

        public decimal? ReferatRgrHours { get; set; }

        public decimal? DiplomaHours { get; set; }

        public decimal? GekHours { get; set; }

        public decimal? PracticeManagementHours { get; set; }

        public decimal? EducationalPracticeHours { get; set; }

        public decimal? IndustrialPracticeHours { get; set; }

        public decimal? PreDiplomaPracticeHours { get; set; }

        public decimal? ScientificPedagogicalPracticeHours { get; set; }

        public decimal? ScientificQualificationWorkHours { get; set; }

        public decimal? ParticipationInGekHours { get; set; }

        public decimal TotalHours { get; set; }
    }
}