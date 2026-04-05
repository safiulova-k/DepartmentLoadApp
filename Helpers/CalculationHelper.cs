using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Enums;

namespace DepartmentLoadApp.Helpers
{
    public static class CalculationHelper
    {
        public static int GetStudentsByCourse(ContingentRow contingent, int course)
        {
            return course switch
            {
                1 => contingent.Course1Count,
                2 => contingent.Course2Count,
                3 => contingent.Course3Count,
                4 => contingent.Course4Count,
                _ => 0
            };
        }

        public static int GetGroupsByCourse(ContingentRow contingent, int course)
        {
            return course switch
            {
                1 => contingent.Course1Groups,
                2 => contingent.Course2Groups,
                3 => contingent.Course3Groups,
                4 => contingent.Course4Groups,
                _ => 0
            };
        }

        public static int GetSubgroupsByCourse(ContingentRow contingent, int course)
        {
            return course switch
            {
                1 => contingent.Course1Subgroups,
                2 => contingent.Course2Subgroups,
                3 => contingent.Course3Subgroups,
                4 => contingent.Course4Subgroups,
                _ => 0
            };
        }

        public static decimal CalculateByNorm(
            WorkCalculationBase calculationBase,
            decimal coefficient,
            int studentsCount = 0,
            int groupCount = 0,
            int subgroupCount = 0,
            int streamCount = 0,
            int weeksCount = 1,
            decimal planHours = 0)
        {
            return calculationBase switch
            {
                WorkCalculationBase.PerStudent => weeksCount * studentsCount * coefficient,
                WorkCalculationBase.PerGroup => weeksCount * groupCount * coefficient,
                WorkCalculationBase.PerSubgroup => weeksCount * subgroupCount * coefficient,
                WorkCalculationBase.PerStream => weeksCount * streamCount * coefficient,
                WorkCalculationBase.PerWork => coefficient,
                WorkCalculationBase.FromLectureHoursTotal => planHours * coefficient,
                _ => 0
            };
        }

        public static decimal RoundHours(decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }
    }
}