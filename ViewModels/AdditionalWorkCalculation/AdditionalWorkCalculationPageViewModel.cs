using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.ViewModels.AdditionalWorkCalculation
{
    public class AdditionalWorkCalculationPageViewModel
    {
        public int SelectedYearStart { get; set; }

        public string SelectedYear { get; set; } = string.Empty;

        public List<int> AvailableYearStarts { get; set; } = new();

        public List<AdditionalWorkCalculationRowViewModel> Rows { get; set; } = new();

        public int TotalHours => Rows.Sum(x => x.TotalHours);
    }

    public class AdditionalWorkCalculationRowViewModel
    {
        public int Id { get; set; }

        public AdditionalWorkType WorkType { get; set; }

        public string Code { get; set; } = string.Empty;

        public string WorkName { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal HoursPerUnit { get; set; }

        public int TotalHours { get; set; }

        public bool IsPostgraduate => WorkType == AdditionalWorkType.PostgraduateSupervision;
    }
}