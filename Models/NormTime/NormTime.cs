using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Models.NormTime
{
    public class NormTime
    {
        public int Id { get; set; }
        public string WorkTypeName { get; set; } = string.Empty;
        public NormCalculationType CalculationType { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal HoursValue { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
    }
}