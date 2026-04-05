using DepartmentLoadApp.Models.Gia;

namespace DepartmentLoadApp.ViewModels.Gia
{
    public class GiaWorkloadPageViewModel
    {
        public string SelectedYear { get; set; } = string.Empty;
        public List<GiaWorkloadRow> Rows { get; set; } = new();
    }
}