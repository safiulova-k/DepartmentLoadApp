namespace DepartmentLoadApp.Models
{
    public class LoadDistribution
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        public Models.Core.Lecturer Lecturer { get; set; }

        public int LoadCalculationId { get; set; }
        public LoadCalculation? LoadCalculation { get; set; }

        public decimal Hours { get; set; }
    }
}