using DepartmentLoadApp.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DepartmentLoadApp.ViewModels.NormTime
{
    public class NormTimePageViewModel
    {
        public List<NormTimeRowViewModel> Items { get; set; } = new();

        public List<SelectListItem> CalculationBaseItems { get; set; } = new()
        {
            new SelectListItem("На поток", ((int)WorkCalculationBase.PerStream).ToString()),
            new SelectListItem("На группу", ((int)WorkCalculationBase.PerGroup).ToString()),
            new SelectListItem("На подгруппу", ((int)WorkCalculationBase.PerSubgroup).ToString()),
            new SelectListItem("На студента", ((int)WorkCalculationBase.PerStudent).ToString()),
            new SelectListItem("За работу", ((int)WorkCalculationBase.PerWork).ToString()),
            new SelectListItem("От общего числа лекционных часов", ((int)WorkCalculationBase.FromLectureHoursTotal).ToString())
        };
    }
}