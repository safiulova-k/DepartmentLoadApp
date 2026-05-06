using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Practice;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services
{
    public class PracticeCalculationService
    {
        private const string PracticeCategoryKeyword = "%практи%";
        private const string ResearchCategoryKeyword = "%науч%";

        private const string TechnologyPracticeKeyword = "технологическ";
        private const string PreDiplomaPracticeKeyword = "преддиплом";
        private const string IntroductoryPracticeKeyword = "ознаком";
        private const string ResearchPracticeShortName = "нир";
        private const string ResearchPracticeKeyword = "научно-исследователь";
        private const string StudyPracticeKeyword = "учебн";

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
                    !string.IsNullOrWhiteSpace(x.CategoryName)
                    && (EF.Functions.ILike(x.CategoryName, PracticeCategoryKeyword)
                        || EF.Functions.ILike(x.CategoryName, ResearchCategoryKeyword)))
                .ToListAsync();

            var contingents = await _context.ContingentRows
                .AsNoTracking()
                .ToListAsync();

            var contingentMap = contingents
                .GroupBy(x => TextNormalizeHelper.Normalize(x.DirectionCode))
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var row in rows)
            {
                var directionCode = TextNormalizeHelper.Normalize(row.DirectionCode);

                if (!contingentMap.TryGetValue(directionCode, out var contingent))
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

        private static NormTime? FindPracticeNorm(
            List<NormTime> norms,
            string practiceName)
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
            {
                return true;
            }

            if (a.Contains(TechnologyPracticeKeyword) && b.Contains(TechnologyPracticeKeyword))
            {
                return true;
            }

            if (a.Contains(PreDiplomaPracticeKeyword) && b.Contains(PreDiplomaPracticeKeyword))
            {
                return true;
            }

            if (a.Contains(IntroductoryPracticeKeyword) && b.Contains(IntroductoryPracticeKeyword))
            {
                return true;
            }

            if (a == ResearchPracticeShortName && b == ResearchPracticeShortName)
            {
                return true;
            }

            if (a.Contains(ResearchPracticeKeyword) && b.Contains(ResearchPracticeKeyword))
            {
                return true;
            }

            if (a.Contains(StudyPracticeKeyword) && b.Contains(StudyPracticeKeyword))
            {
                return true;
            }

            return false;
        }

        private static string NormalizePracticeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Replace("бакалавров", string.Empty)
                .Replace("магистров", string.Empty)
                .Replace("(учебная)", string.Empty)
                .Replace("(производственная)", string.Empty);

            return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}