using DepartmentLoadApp.Models.Contingent;
using System.Linq;

namespace DepartmentLoadApp.ViewModels.Contingent
{
    public class ContingentPageViewModel
    {
        public List<ContingentRow> Rows { get; set; } = new();

        public int TotalCourse1 => Rows.Sum(x => x.Course1Count);
        public int TotalCourse2 => Rows.Sum(x => x.Course2Count);
        public int TotalCourse3 => Rows.Sum(x => x.Course3Count);
        public int TotalCourse4 => Rows.Sum(x => x.Course4Count);

        public int TotalStudents =>
            TotalCourse1 +
            TotalCourse2 +
            TotalCourse3 +
            TotalCourse4;

        public int BachelorCourse1 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course1Count);
        public int BachelorCourse2 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course2Count);
        public int BachelorCourse3 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course3Count);
        public int BachelorCourse4 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course4Count);

        public int MasterCourse1 => Rows.Where(x => x.IsMaster).Sum(x => x.Course1Count);
        public int MasterCourse2 => Rows.Where(x => x.IsMaster).Sum(x => x.Course2Count);
        public int MasterCourse3 => Rows.Where(x => x.IsMaster).Sum(x => x.Course3Count);
        public int MasterCourse4 => Rows.Where(x => x.IsMaster).Sum(x => x.Course4Count);
    }
}