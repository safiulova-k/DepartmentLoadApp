using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Gia;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services
{
    public class GiaCalculationService
    {
        private readonly DepartmentLoadDbContext _context;

        public GiaCalculationService(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        public async Task RecalculateAsync(List<GiaWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x => x.CategoryName == "ГИА")
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
                row.TotalHours = CalculateGiaHours(row, norms, contingent);
            }
        }

        private static decimal CalculateGiaHours(
            GiaWorkloadRow row,
            List<NormTime> norms,
            ContingentRow contingent)
        {
            if (row.WorkName == "Консультация к госэкзамену")
            {
                return CalculationHelper.RoundHours(row.ManualHours);
            }

            var normName = GetGiaNormName(row, contingent);
            var norm = norms.FirstOrDefault(x => x.WorkName == normName);

            if (norm == null)
            {
                return 0;
            }

            var result = CalculationHelper.CalculateByNorm(
                calculationBase: norm.CalculationBase,
                coefficient: norm.Hours,
                studentsCount: row.StudentsCount,
                groupCount: row.GroupCount);

            return CalculationHelper.RoundHours(result);
        }

        private static string GetGiaNormName(GiaWorkloadRow row, ContingentRow contingent)
        {
            if (row.WorkName == "Руководство ВКР")
            {
                return contingent.IsMaster
                    ? "Руководство ВКР магистра"
                    : "Руководство ВКР бакалавра";
            }

            return row.WorkName;
        }

        private static void ResetCalculatedFields(GiaWorkloadRow row)
        {
            row.StudentsCount = 0;
            row.GroupCount = 0;
            row.TotalHours = 0;
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