using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Models.AdditionalWork
{
    public class AdditionalWorkNorm
    {
        public int Id { get; set; }

        public AdditionalWorkType WorkType { get; set; }

        [Required]
        [StringLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Hours { get; set; }

        public bool IsDefault { get; set; }
    }
}