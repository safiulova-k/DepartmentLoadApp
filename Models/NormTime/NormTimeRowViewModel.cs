using DepartmentLoadApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DepartmentLoadApp.Models.NormTime
{
    public class NormTimeRowViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название вида нагрузки обязательно")]
        [Display(Name = "Вид нагрузки")]
        public string WorkTypeName { get; set; } = string.Empty;

        [Display(Name = "Единица расчета")]
        public string UnitName { get; set; } = string.Empty;

        [Display(Name = "Тип расчета")]
        public NormCalculationType CalculationType { get; set; }

        [Range(0, 10000, ErrorMessage = "Часы должны быть неотрицательными")]
        [Display(Name = "Норма времени, часов")]
        public decimal HoursValue { get; set; }

        [Display(Name = "Комментарий")]
        public string? Note { get; set; }

        [Display(Name = "Активно")]
        public bool IsActive { get; set; } = true;
    }
}