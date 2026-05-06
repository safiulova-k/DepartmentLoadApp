using DepartmentLoadApp.Models.Contingent;

namespace DepartmentLoadApp.ViewModels.Contingent;

public class ContingentPageViewModel
{
    public List<ContingentRow> Rows { get; set; } = new();

    public List<ContingentDirectionViewModel> Directions { get; set; } = new();

    public int TotalCourse1 => Rows.Sum(x => x.Course1Count);
    public int TotalCourse2 => Rows.Sum(x => x.Course2Count);
    public int TotalCourse3 => Rows.Sum(x => x.Course3Count);
    public int TotalCourse4 => Rows.Sum(x => x.Course4Count);

    public int TotalStudents => TotalCourse1 + TotalCourse2 + TotalCourse3 + TotalCourse4;

    public int BachelorCourse1 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course1Count);
    public int BachelorCourse2 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course2Count);
    public int BachelorCourse3 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course3Count);
    public int BachelorCourse4 => Rows.Where(x => x.IsBachelor).Sum(x => x.Course4Count);

    public int MasterCourse1 => Rows.Where(x => x.IsMaster).Sum(x => x.Course1Count);
    public int MasterCourse2 => Rows.Where(x => x.IsMaster).Sum(x => x.Course2Count);
    public int MasterCourse3 => Rows.Where(x => x.IsMaster).Sum(x => x.Course3Count);
    public int MasterCourse4 => Rows.Where(x => x.IsMaster).Sum(x => x.Course4Count);
}

public class ContingentDirectionViewModel
{
    public string DirectionCode { get; set; } = string.Empty;

    public bool IsBachelor { get; set; }

    public bool IsMaster => !IsBachelor;

    public string QualificationName { get; set; } = string.Empty;

    public List<ContingentCourseViewModel> Courses { get; set; } = new();
    public string DirectionName { get; set; } = string.Empty;
}

public class ContingentCourseViewModel
{
    public int CourseNumber { get; set; }

    public List<ContingentGroupViewModel> Groups { get; set; } = new();
}

public class ContingentGroupViewModel
{
    public int StudentGroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public int StudentsCount { get; set; }

    public List<ContingentSubgroupViewModel> Subgroups { get; set; } = new();

    public int SubgroupsCount => Subgroups.Count;
}

public class ContingentSubgroupViewModel
{
    public int Id { get; set; }

    public int StudentGroupId { get; set; }

    public int SubgroupNumber { get; set; }

    public int StudentsCount { get; set; }

    public string Name => $"{SubgroupNumber}п/г";
}