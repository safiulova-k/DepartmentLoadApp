namespace DepartmentLoadApp.Helpers
{
    public static class TextNormalizeHelper
    {
        public static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(' ', value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}