namespace DepartmentLoadApp.Models
{
    public class LoadCalculation
    {
        public int Id { get; set; }
        public string LoadType { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int GroupCount { get; set; }
        public decimal TotalHours { get; set; }

        public ICollection<LoadDistribution> LoadDistributions { get; set; } = new List<LoadDistribution>();
    }
}