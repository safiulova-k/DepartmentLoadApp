using System.ComponentModel.DataAnnotations;
using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.ViewModels.NormTime
{
    public class NormTimeRowViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Вид работы")]
        public string WorkName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Основа расчета")]
        public WorkCalculationBase CalculationBase { get; set; }

        [Display(Name = "Часы")]
        [Range(0, 9999)]
        public decimal Hours { get; set; }
    }
}