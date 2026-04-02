using System.ComponentModel.DataAnnotations;

namespace DepartmentLoadApp.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        public int? ExternalLecturerId { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        public ICollection<LoadDistribution> LoadDistributions { get; set; } = new List<LoadDistribution>();
    }
}