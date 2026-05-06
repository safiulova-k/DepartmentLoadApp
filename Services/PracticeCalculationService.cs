using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Practice;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services
{
    public class PracticeCalculationService
    {
        private readonly DepartmentLoadDbContext _context;

        public PracticeCalculationService(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        public async Task RecalculateAsync(List<PracticeWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.CategoryName) &&
                    (EF.Functions.ILike(x.CategoryName, "%практи%") ||
                     EF.Functions.ILike(x.CategoryName, "%науч%")))
                .ToListAsync();

            var contingents = await _context.ContingentRows
                .AsNoTracking()
                .ToListAsync();

            var contingentMap = contingents
                .GroupBy(x => NormalizeText(x.DirectionCode))
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var row in rows)
            {
                if (!contingentMap.TryGetValue(NormalizeText(row.DirectionCode), out var contingent))
                {
                    ResetCalculatedFields(row);
                    continue;
                }

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);

                var norm = FindPracticeNorm(norms, row.PracticeName);

                if (norm == null || row.WeeksCount <= 0 || norm.Hours <= 0)
                {
                    row.TotalHours = 0;
                    continue;
                }

                var result = CalculationHelper.CalculateByNorm(
                    calculationBase: norm.CalculationBase,
                    coefficient: norm.Hours,
                    studentsCount: row.StudentsCount,
                    groupCount: row.GroupCount,
                    weeksCount: row.WeeksCount);

                row.TotalHours = CalculationHelper.RoundHours(result);
            }
        }

        private static void ResetCalculatedFields(PracticeWorkloadRow row)
        {
            row.StudentsCount = 0;
            row.GroupCount = 0;
            row.TotalHours = 0;
        }

        private static NormTime? FindPracticeNorm(List<NormTime> norms, string practiceName)
        {
            var target = NormalizePracticeKey(practiceName);

            return norms.FirstOrDefault(x => NormalizePracticeKey(x.WorkName) == target)
                ?? norms.FirstOrDefault(x => IsSamePracticeType(x.WorkName, practiceName));
        }

        private static bool IsSamePracticeType(string? left, string? right)
        {
            var a = NormalizePracticeKey(left);
            var b = NormalizePracticeKey(right);

            if (a == b)
                return true;

            if (a.Contains("технологическ") && b.Contains("технологическ"))
                return true;

            if (a.Contains("преддиплом") && b.Contains("преддиплом"))
                return true;

            if (a.Contains("ознаком") && b.Contains("ознаком"))
                return true;

            if (a == "нир" && b == "нир")
                return true;

            if (a.Contains("научно-исследователь") && b.Contains("научно-исследователь"))
                return true;

            if (a.Contains("учебн") && b.Contains("учебн"))
                return true;

            return false;
        }

        private static string NormalizePracticeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Replace("бакалавров", "")
                .Replace("магистров", "")
                .Replace("(учебная)", "")
                .Replace("(производственная)", "");

            return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return string.Join(' ', value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}