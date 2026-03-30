using DepartmentLoadApp.Models.Practice;

namespace DepartmentLoadApp.ViewModels.Practice
{
    public class PracticeWorkloadPageViewModel
    {
        public int SelectedYear { get; set; }

        public List<PracticeWorkloadRow> Rows { get; set; } = new();
    }
}