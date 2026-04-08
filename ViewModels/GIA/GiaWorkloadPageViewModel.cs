using DepartmentLoadApp.Models.Gia;

namespace DepartmentLoadApp.ViewModels.Gia
{
    public class GiaWorkloadPageViewModel
    {
        public int SelectedYearStart { get; set; }
        public IReadOnlyList<int> AvailableYearStarts { get; set; } = Array.Empty<int>();
        public string SelectedYear { get; set; } = string.Empty;
        public List<GiaWorkloadRow> Rows { get; set; } = new();
    }
}