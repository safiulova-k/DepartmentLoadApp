using DepartmentLoadApp.Models.Gia;

namespace DepartmentLoadApp.ViewModels.Gia
{
    public class GiaWorkloadPageViewModel
    {
        public int SelectedYear { get; set; }
        public List<GiaWorkloadRow> Rows { get; set; } = new();
    }
}