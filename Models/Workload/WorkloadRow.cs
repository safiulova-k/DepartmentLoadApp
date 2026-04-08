using System.ComponentModel.DataAnnotations;

namespace DepartmentLoadApp.Models.Workload
{
    public class WorkloadRow
    {
        public int Id { get; set; }

        [Required]
        [StringLength(9)]
        public string AcademicYear { get; set; } = string.Empty;

        public int AcademicPlanId { get; set; }
        public int AcademicPlanRecordId { get; set; }

        public int DisciplineId { get; set; }

        [StringLength(100)]
        public string RecordIndex { get; set; } = string.Empty;

        public string DisciplineName { get; set; } = string.Empty;
        public string DirectionCode { get; set; } = string.Empty;
        public string DirectionName { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;
        public string EducationForm { get; set; } = string.Empty;

        public bool IsFacultyOptional { get; set; }

        public int Course { get; set; }

        public int StudentsCount { get; set; }
        public int FlowCount { get; set; }
        public int GroupCount { get; set; }
        public int SubgroupCount { get; set; }

        public decimal LecturePlanHours { get; set; }
        public decimal LectureTotalHours { get; set; }

        public decimal PracticePlanHours { get; set; }
        public decimal PracticeTotalHours { get; set; }

        public decimal LabPlanHours { get; set; }
        public decimal LabTotalHours { get; set; }

        public bool HasExam { get; set; }
        public bool HasCredit { get; set; }
        public bool HasCourseWork { get; set; }
        public bool HasCourseProject { get; set; }
        public bool HasRgr { get; set; }

        public decimal ConsultationHours { get; set; }
        public decimal ExamHours { get; set; }
        public decimal CreditHours { get; set; }
        public decimal CourseWorkHours { get; set; }
        public decimal CourseProjectHours { get; set; }
        public decimal RgrHours { get; set; }

        public decimal TotalHours =>
            LectureTotalHours +
            PracticeTotalHours +
            LabTotalHours +
            ConsultationHours +
            ExamHours +
            CreditHours +
            CourseWorkHours +
            CourseProjectHours +
            RgrHours;
    }
}