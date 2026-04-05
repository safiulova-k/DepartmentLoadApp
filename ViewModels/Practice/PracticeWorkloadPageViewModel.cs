using DepartmentLoadApp.Models.Practice;

namespace DepartmentLoadApp.ViewModels.Practice
{
    public class PracticeWorkloadPageViewModel
    {
        public string SelectedYear { get; set; } = string.Empty;
        public List<PracticeWorkloadRow> Rows { get; set; } = new();
    }
}