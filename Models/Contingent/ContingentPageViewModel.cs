using System.Collections.Generic;
using System.Linq;

namespace DepartmentLoadApp.Models.Contingent
{
    public class ContingentPageViewModel
    {
        public List<ContingentDirectionRowViewModel> Rows { get; set; } = new();

        // суммы по курсам
        public int TotalCourse1 => Rows.Sum(x => x.Course1Count);
        public int TotalCourse2 => Rows.Sum(x => x.Course2Count);
        public int TotalCourse3 => Rows.Sum(x => x.Course3Count);
        public int TotalCourse4 => Rows.Sum(x => x.Course4Count);

        // общее количество студентов
        public int TotalStudents =>
            TotalCourse1 +
            TotalCourse2 +
            TotalCourse3 +
            TotalCourse4;

        // бакалавры
        public int BachelorCourse1 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course1Count);
        public int BachelorCourse2 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course2Count);
        public int BachelorCourse3 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course3Count);
        public int BachelorCourse4 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course4Count);

        // магистры
        public int MasterCourse1 => Rows.Where(x => x.IsMaster).Sum(x => x.Course1Count);
        public int MasterCourse2 => Rows.Where(x => x.IsMaster).Sum(x => x.Course2Count);
        public int MasterCourse3 => Rows.Where(x => x.IsMaster).Sum(x => x.Course3Count);
        public int MasterCourse4 => Rows.Where(x => x.IsMaster).Sum(x => x.Course4Count);
    }
}