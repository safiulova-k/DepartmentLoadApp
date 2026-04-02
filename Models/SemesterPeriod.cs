using System.ComponentModel.DataAnnotations;

namespace DepartmentLoadApp.Models
{
    public class SemesterPeriod
    {
        public int Id { get; set; }

        [Required]
        [StringLength(9)]
        public string AcademicYear { get; set; } = string.Empty; // 2025-2026

        [Required]
        [StringLength(20)]
        public string Season { get; set; } = string.Empty; // Осень / Весна

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}