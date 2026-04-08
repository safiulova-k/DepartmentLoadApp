using DepartmentLoadApp.Models.Practice;

namespace DepartmentLoadApp.ViewModels.Practice
{
    public class PracticeWorkloadPageViewModel
    {
        public int SelectedYearStart { get; set; }
        public IReadOnlyList<int> AvailableYearStarts { get; set; } = Array.Empty<int>();
        public string SelectedYear { get; set; } = string.Empty;
        public List<PracticeWorkloadRow> Rows { get; set; } = new();
    }
}