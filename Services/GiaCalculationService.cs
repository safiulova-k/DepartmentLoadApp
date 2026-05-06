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
        private const string GiaCategoryName = "ГИА";
        private const string StateExamConsultationWorkName = "Консультация к госэкзамену";
        private const string FinalQualificationWorkName = "Руководство ВКР";
        private const string MasterFinalQualificationWorkNormName = "Руководство ВКР магистра";
        private const string BachelorFinalQualificationWorkNormName = "Руководство ВКР бакалавра";

        private readonly DepartmentLoadDbContext _context;

        public GiaCalculationService(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        public async Task RecalculateAsync(List<GiaWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x => x.CategoryName == GiaCategoryName)
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
                row.TotalHours = CalculateGiaHours(row, norms, contingent);
            }
        }

        private static decimal CalculateGiaHours(
            GiaWorkloadRow row,
            List<NormTime> norms,
            ContingentRow contingent)
        {
            if (row.WorkName == StateExamConsultationWorkName)
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

        private static string GetGiaNormName(
            GiaWorkloadRow row,
            ContingentRow contingent)
        {
            if (row.WorkName == FinalQualificationWorkName)
            {
                return contingent.IsMaster
                    ? MasterFinalQualificationWorkNormName
                    : BachelorFinalQualificationWorkNormName;
            }

            return row.WorkName;
        }

        private static void ResetCalculatedFields(GiaWorkloadRow row)
        {
            row.StudentsCount = 0;
            row.GroupCount = 0;
            row.TotalHours = 0;
        }
    }
}