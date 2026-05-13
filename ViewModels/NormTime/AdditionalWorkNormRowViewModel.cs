using System.ComponentModel.DataAnnotations;
using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.ViewModels.NormTime
{
    public class AdditionalWorkNormRowViewModel
    {
        public int Id { get; set; }

        public AdditionalWorkType WorkType { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        [Range(0, 9999)]
        public decimal Hours { get; set; }
    }
}