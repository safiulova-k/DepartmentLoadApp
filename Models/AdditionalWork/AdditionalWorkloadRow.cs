using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Models.AdditionalWork
{
    public class AdditionalWorkloadRow
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        public AdditionalWorkType WorkType { get; set; }

        public int? AdditionalWorkNormId { get; set; }

        public AdditionalWorkNorm? AdditionalWorkNorm { get; set; }

        [Required]
        [StringLength(500)]
        public string WorkName { get; set; } = string.Empty;

        public int Count { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal HoursPerUnit { get; set; }

        public int TotalHours { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
    }
}