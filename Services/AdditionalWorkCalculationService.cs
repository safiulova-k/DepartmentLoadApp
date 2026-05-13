using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models.AdditionalWork;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.ViewModels.AdditionalWorkCalculation;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services
{
    public class AdditionalWorkCalculationService
    {
        private const string PostgraduateNormCode = "POSTGRADUATE_SUPERVISION";
        private const string DepartmentHeadNormCode = "DEPARTMENT_HEAD";
        private const string DeputyDeanAcademicNormCode = "DEPUTY_DEAN_ACADEMIC";
        private const string DeputyDeanEducationalNormCode = "DEPUTY_DEAN_EDUCATIONAL";

        private static readonly string[] AdditionalWorkCodes =
        {
            PostgraduateNormCode,
            DepartmentHeadNormCode,
            DeputyDeanAcademicNormCode,
            DeputyDeanEducationalNormCode
        };

        private readonly DepartmentLoadDbContext _context;

        public AdditionalWorkCalculationService(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        public async Task<AdditionalWorkCalculationPageViewModel> BuildPageAsync(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var academicYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            await EnsureDefaultNormsAsync();
            await EnsureRowsAsync(academicYear);
            await RecalculateAndSaveAsync(academicYear);

            var rows = await LoadRowsAsync(academicYear);

            return new AdditionalWorkCalculationPageViewModel
            {
                SelectedYearStart = selectedYearStart,
                SelectedYear = academicYear,
                AvailableYearStarts = AcademicYearResolver.BuildAvailableStartYears(selectedYearStart).ToList(),
                Rows = rows.Select(MapRow).ToList()
            };
        }

        public async Task SavePostgraduateCountAsync(int selectedYearStart, int count)
        {
            var academicYear = AcademicYearResolver.BuildAcademicYear(
                AcademicYearResolver.NormalizeStartYear(selectedYearStart));

            await EnsureDefaultNormsAsync();
            await EnsureRowsAsync(academicYear);

            var row = await _context.AdditionalWorkloadRows
                .Include(x => x.AdditionalWorkNorm)
                .FirstOrDefaultAsync(x =>
                    x.AcademicYear == academicYear &&
                    x.AdditionalWorkNorm != null &&
                    x.AdditionalWorkNorm.Code == PostgraduateNormCode);

            if (row == null)
            {
                return;
            }

            row.Count = Math.Max(0, count);

            await RecalculateAndSaveAsync(academicYear);
        }

        public async Task<List<AdditionalWorkDistributionItem>> BuildDistributionItemsAsync(
            string academicYear)
        {
            await EnsureDefaultNormsAsync();
            await EnsureRowsAsync(academicYear);
            await RecalculateAndSaveAsync(academicYear);

            var rows = await LoadRowsAsync(academicYear);

            return rows
             .Where(x => x.TotalHours > 0)
             .Select(x => new AdditionalWorkDistributionItem
             {
                 SourceRowId = x.Id,
                 SourceAcademicPlanRecordId = x.Id,
                 WorkType = x.WorkType,
                 LoadElementType = x.WorkType == AdditionalWorkType.PostgraduateSupervision
                     ? LoadAssignmentElementType.PostgraduateSupervision
                     : LoadAssignmentElementType.OrganizationalWork,
                 Title = "Доп. работа",
                 Subtitle = x.WorkType == AdditionalWorkType.PostgraduateSupervision
                     ? $"Всего аспирантов: {x.Count}, норма: {x.HoursPerUnit:0.##} ч."
                     : $"Доступно часов: {x.TotalHours:0.##}",
                 ElementDisplayName = x.WorkName,
                 Count = x.Count,
                 HoursPerUnit = x.HoursPerUnit,
                 TotalHours = x.TotalHours
             })
             .ToList();
        }

        private async Task EnsureDefaultNormsAsync()
        {
            var existingItems = await _context.AdditionalWorkNorms
                .ToListAsync();

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.PostgraduateSupervision,
                PostgraduateNormCode,
                "Аспиранты");

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.OrganizationalWork,
                DepartmentHeadNormCode,
                "Руководство кафедрой");

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.OrganizationalWork,
                DeputyDeanAcademicNormCode,
                "Зам декана по учебной работе");

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.OrganizationalWork,
                DeputyDeanEducationalNormCode,
                "Зам декана по воспит.работе");

            await _context.SaveChangesAsync();
        }

        private void AddOrUpdateNorm(
            List<AdditionalWorkNorm> existingItems,
            AdditionalWorkType workType,
            string code,
            string name)
        {
            var existingItem = existingItems
                .FirstOrDefault(x => x.Code == code);

            if (existingItem == null)
            {
                _context.AdditionalWorkNorms.Add(new AdditionalWorkNorm
                {
                    WorkType = workType,
                    Code = code,
                    Name = name,
                    Hours = 0m,
                    IsDefault = false
                });

                return;
            }

            existingItem.WorkType = workType;
            existingItem.Name = name;
            existingItem.IsDefault = false;
        }

        private async Task EnsureRowsAsync(string academicYear)
        {
            var norms = await _context.AdditionalWorkNorms
                .Where(x => AdditionalWorkCodes.Contains(x.Code))
                .ToListAsync();

            var existingRows = await _context.AdditionalWorkloadRows
                .Include(x => x.AdditionalWorkNorm)
                .Where(x => x.AcademicYear == academicYear)
                .ToListAsync();

            foreach (var norm in norms)
            {
                var row = existingRows.FirstOrDefault(x =>
                    x.AdditionalWorkNormId == norm.Id);

                if (row != null)
                {
                    row.WorkType = norm.WorkType;
                    row.WorkName = norm.Name;
                    continue;
                }

                _context.AdditionalWorkloadRows.Add(new AdditionalWorkloadRow
                {
                    AcademicYear = academicYear,
                    WorkType = norm.WorkType,
                    AdditionalWorkNormId = norm.Id,
                    WorkName = norm.Name,
                    Count = norm.WorkType == AdditionalWorkType.PostgraduateSupervision ? 0 : 1,
                    HoursPerUnit = norm.Hours,
                    TotalHours = 0
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task RecalculateAndSaveAsync(string academicYear)
        {
            var rows = await _context.AdditionalWorkloadRows
                .Include(x => x.AdditionalWorkNorm)
                .Where(x => x.AcademicYear == academicYear)
                .Where(x => x.AdditionalWorkNorm != null)
                .Where(x => AdditionalWorkCodes.Contains(x.AdditionalWorkNorm!.Code))
                .ToListAsync();

            foreach (var row in rows)
            {
                var norm = row.AdditionalWorkNorm!;

                row.WorkType = norm.WorkType;
                row.WorkName = norm.Name;
                row.HoursPerUnit = norm.Hours;

                if (row.WorkType == AdditionalWorkType.PostgraduateSupervision)
                {
                    row.Count = Math.Max(0, row.Count);
                    row.TotalHours = RoundHoursToInt(row.Count * row.HoursPerUnit);
                }
                else
                {
                    row.Count = 1;
                    row.TotalHours = RoundHoursToInt(row.HoursPerUnit);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<List<AdditionalWorkloadRow>> LoadRowsAsync(string academicYear)
        {
            var rows = await _context.AdditionalWorkloadRows
                .AsNoTracking()
                .Include(x => x.AdditionalWorkNorm)
                .Where(x => x.AcademicYear == academicYear)
                .Where(x => x.AdditionalWorkNorm != null)
                .Where(x => AdditionalWorkCodes.Contains(x.AdditionalWorkNorm!.Code))
                .ToListAsync();

            return rows
                .OrderBy(x => GetAdditionalWorkSortOrder(x.AdditionalWorkNorm!.Code))
                .ToList();
        }

        private static AdditionalWorkCalculationRowViewModel MapRow(
            AdditionalWorkloadRow row)
        {
            return new AdditionalWorkCalculationRowViewModel
            {
                Id = row.Id,
                WorkType = row.WorkType,
                Code = row.AdditionalWorkNorm?.Code ?? string.Empty,
                WorkName = row.WorkName,
                Count = row.Count,
                HoursPerUnit = row.HoursPerUnit,
                TotalHours = row.TotalHours
            };
        }

        private static int GetAdditionalWorkSortOrder(string code)
        {
            return code switch
            {
                PostgraduateNormCode => 1,
                DepartmentHeadNormCode => 2,
                DeputyDeanAcademicNormCode => 3,
                DeputyDeanEducationalNormCode => 4,
                _ => 100
            };
        }

        private static int RoundHoursToInt(decimal value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }
    }

    public class AdditionalWorkDistributionItem
    {
        public int SourceRowId { get; set; }

        public int SourceAcademicPlanRecordId { get; set; }

        public AdditionalWorkType WorkType { get; set; }

        public LoadAssignmentElementType LoadElementType { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public string ElementDisplayName { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal HoursPerUnit { get; set; }

        public int TotalHours { get; set; }
    }
}