using System.ComponentModel.DataAnnotations;

namespace DepartmentLoadApp.Models
{
    public class StudentFlow
    {
        public int Id { get; set; }

        [Required]
        [StringLength(9)]
        public string AcademicYear { get; set; } = string.Empty; // 2025-2026

        [Required]
        [StringLength(200)]
        public string FlowName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DirectionCode { get; set; } = string.Empty;

        [Required]
        public int Course { get; set; }

        [Required]
        [StringLength(50)]
        public string EducationLevel { get; set; } = string.Empty; // Бакалавриат / Магистратура

        [StringLength(500)]
        public string GroupNames { get; set; } = string.Empty;

        [Required]
        public int StudentsCount { get; set; }

        [Required]
        public int GroupsCount { get; set; }
    }
}