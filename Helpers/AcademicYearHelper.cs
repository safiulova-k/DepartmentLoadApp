namespace DepartmentLoadApp.Helpers
{
    public static class AcademicYearHelper
    {
        public static string GetCurrentAcademicYear(DateTime? today = null)
        {
            var date = today ?? DateTime.Today;
            var startYear = date.Month >= 9 ? date.Year : date.Year - 1;

            return $"{startYear}-{startYear + 1}";
        }
    }
}