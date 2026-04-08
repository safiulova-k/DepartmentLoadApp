namespace DepartmentLoadApp.ViewModels.Shared
{
    public class CalculationTopPanelViewModel
    {
        public int SelectedYearStart { get; set; }
        public string SelectedYear { get; set; } = string.Empty;
        public IReadOnlyList<int> AvailableYearStarts { get; set; } = Array.Empty<int>();
        public string ActiveTab { get; set; } = string.Empty;
    }
}