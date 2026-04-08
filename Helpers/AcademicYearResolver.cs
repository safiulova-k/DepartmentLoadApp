using System.Text.RegularExpressions;

namespace DepartmentLoadApp.Helpers
{
    public static class AcademicYearResolver
    {
        private static readonly Regex YearRangeRegex =
            new(@"^(?<start>\d{4})-(?<end>\d{4})$", RegexOptions.Compiled);

        public static int GetCurrentAcademicYearStart()
        {
            var now = DateTime.Now;
            return now.Month >= 9 ? now.Year : now.Year - 1;
        }

        public static int NormalizeStartYear(int? startYear)
        {
            var current = GetCurrentAcademicYearStart();

            if (!startYear.HasValue)
                return current;

            if (startYear.Value < 2000 || startYear.Value > current + 10)
                return current;

            return startYear.Value;
        }

        public static string BuildAcademicYear(int startYear)
        {
            return $"{startYear}-{startYear + 1}";
        }

        public static IReadOnlyList<int> BuildAvailableStartYears(int selectedStartYear, int before = 6, int after = 3)
        {
            var result = new List<int>();

            for (int year = selectedStartYear - before; year <= selectedStartYear + after; year++)
            {
                result.Add(year);
            }

            return result;
        }

        public static bool TryParsePlanPeriod(string? value, out int startYear, out int endYear)
        {
            startYear = 0;
            endYear = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var match = YearRangeRegex.Match(value.Trim());
            if (!match.Success)
                return false;

            startYear = int.Parse(match.Groups["start"].Value);
            endYear = int.Parse(match.Groups["end"].Value);

            return endYear > startYear;
        }

        public static bool TryResolveCourseAndSemesters(
            string? planPeriod,
            int calculationYearStart,
            out int course,
            out int[] semesters)
        {
            course = 0;
            semesters = Array.Empty<int>();

            if (!TryParsePlanPeriod(planPeriod, out var planStartYear, out var planEndYear))
                return false;

            var totalCourses = planEndYear - planStartYear;
            if (totalCourses <= 0)
                return false;

            course = calculationYearStart - planStartYear + 1;

            if (course < 1 || course > totalCourses)
                return false;

            semesters = new[]
            {
                course * 2 - 1,
                course * 2
            };

            return true;
        }

        public static string GetSemesterName(int semester)
        {
            return semester % 2 == 0 ? "Весна" : "Осень";
        }
    }
}