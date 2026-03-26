using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Models
{
    public class NormTime
    {
        public int Id { get; set; }

        public string WorkName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public WorkCalculationBase CalculationBase { get; set; }

        public decimal Hours { get; set; }

        public int SortOrder { get; set; }
    }
}