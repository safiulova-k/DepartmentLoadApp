using System.ComponentModel.DataAnnotations;

namespace DepartmentLoadApp.Models.AcademicPlan
{
    public class Discipline
    {
        public int Id { get; set; }

        [Required]
        public int DisciplineBlockId { get; set; }

        [Required]
        [StringLength(300)]
        public string DisciplineName { get; set; } = string.Empty;

        [StringLength(100)]
        public string DisciplineShortName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string DisciplineDescription { get; set; } = string.Empty;

        [StringLength(200)]
        public string DisciplineBlockBlueAsteriskName { get; set; } = string.Empty;

        public List<AcademicPlanRecord> AcademicPlanRecords { get; set; } = new();
    }
}