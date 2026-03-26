namespace DepartmentLoadApp.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Degree { get; set; }

        public ICollection<LoadDistribution> LoadDistributions { get; set; } = new List<LoadDistribution>();
    }
}