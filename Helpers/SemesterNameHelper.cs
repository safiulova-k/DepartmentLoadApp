namespace DepartmentLoadApp.Helpers
{
    public static class SemesterNameHelper
    {
        public static string FromNumber(int semester)
        {
            if (semester <= 0)
                return string.Empty;

            return semester % 2 == 0 ? "весна" : "осень";
        }
    }
}