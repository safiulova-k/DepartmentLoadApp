using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Models.AcademicPlan
{
    public class AcademicPlan
    {
        public int Id { get; set; }

        public int EducationDirectionId { get; set; }

        public AcademicCourse AcademicCourses { get; set; }

        public int Year { get; set; }

        public List<AcademicPlanRecord> AcademicPlanRecords { get; set; } = new();
    }
}