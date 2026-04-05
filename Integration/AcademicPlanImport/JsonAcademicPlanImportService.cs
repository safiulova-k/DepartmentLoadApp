using System.Text.Json;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.AcademicPlanImport.Models;
using DepartmentLoadApp.Models.AcademicPlan;
using DepartmentLoadApp.Models.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.AcademicPlanImport
{
    public class JsonAcademicPlanImportService : IAcademicPlanImportService
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public JsonAcademicPlanImportService(
            DepartmentLoadDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<string?> GetLatestYearAsync()
        {
            var combinedImport = await LoadCombinedImportAsync();
            if (combinedImport != null && combinedImport.AcademicPlans.Count > 0)
            {
                var latestStartYear = combinedImport.AcademicPlans.Max(x => x.Year);
                return ToAcademicYearString(latestStartYear);
            }

            var legacyPlans = await LoadLegacyAcademicPlansAsync();
            if (legacyPlans.Count == 0)
            {
                return null;
            }

            return legacyPlans
                .Select(x => x.Year)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .OrderByDescending(ParseAcademicYearStart)
                .FirstOrDefault();
        }

        public async Task EnsureYearImportedAsync(string year)
        {
            var normalizedYear = NormalizeAcademicYear(year);

            var isYearImported = await _context.AcademicPlans
                .AnyAsync(x => x.Year == normalizedYear);

            if (!isYearImported)
            {
                await ImportYearAsync(normalizedYear);
            }
        }

        public async Task ImportYearAsync(string year)
        {
            var normalizedYear = NormalizeAcademicYear(year);

            var combinedImport = await LoadCombinedImportAsync();

            if (combinedImport != null && combinedImport.AcademicPlans.Count > 0)
            {
                await ImportFromCombinedFileAsync(normalizedYear, combinedImport);
                return;
            }

            await ImportFromSplitFilesAsync(normalizedYear);
        }

        private async Task ImportFromCombinedFileAsync(string year, AcademicPlanImportFileModel importFile)
        {
            var selectedPlans = importFile.AcademicPlans
                .Where(x => ToAcademicYearString(x.Year) == year)
                .ToList();

            if (selectedPlans.Count == 0)
            {
                return;
            }

            await RemoveExistingYearAsync(year);

            await UpsertDisciplinesAsync(importFile.Disciplines);

            var planIds = selectedPlans.Select(x => x.Id).ToHashSet();

            var selectedRecords = importFile.AcademicPlanRecords
                .Where(x => planIds.Contains(x.AcademicPlanId))
                .ToList();

            var recordIds = selectedRecords.Select(x => x.Id).ToHashSet();

            var selectedElements = importFile.AcademicPlanRecordElements
                .Where(x => recordIds.Contains(x.AcademicPlanRecordId))
                .ToList();

            var academicPlans = selectedPlans
                .Select(x => new AcademicPlan
                {
                    Id = x.Id,
                    EducationDirectionId = x.EducationDirectionId,
                    AcademicCourses = MapAcademicCourse(x.AcademicCourses),
                    Year = ToAcademicYearString(x.Year)
                })
                .ToList();

            var academicPlanRecords = selectedRecords
                .Select(x => new AcademicPlanRecord
                {
                    Id = x.Id,
                    AcademicPlanId = x.AcademicPlanId,
                    DisciplineId = x.DisciplineId,
                    AcademicPlanRecordParentId = x.AcademicPlanRecordParentId,
                    InDepartment = x.InDepartment,
                    Semester = (Semester)x.Semester,
                    Zet = x.Zet,
                    IsParent = x.IsParent,
                    IsChild = x.IsChild,
                    IsFacultative = x.IsFacultative,
                    IsUseInWorkload = x.IsUseInWorkload,
                    IsActiveSemester = x.IsActiveSemester,
                    DisciplineBlockId = 0
                })
                .ToList();

            var academicPlanRecordElements = selectedElements
                .Select(x => new AcademicPlanRecordElement
                {
                    Id = x.Id,
                    AcademicPlanRecordId = x.AcademicPlanRecordId,
                    ActivityType = x.ActivityType,
                    PlanHours = x.PlanHours,
                    FactHours = x.FactHours
                })
                .ToList();

            await _context.AcademicPlans.AddRangeAsync(academicPlans);
            await _context.AcademicPlanRecords.AddRangeAsync(academicPlanRecords);
            await _context.AcademicPlanRecordElements.AddRangeAsync(academicPlanRecordElements);

            await _context.SaveChangesAsync();
        }

        private async Task ImportFromSplitFilesAsync(string year)
        {
            var legacyPlans = await LoadLegacyAcademicPlansAsync();

            var selectedPlans = legacyPlans
                .Where(x => x.Year == year)
                .ToList();

            if (selectedPlans.Count == 0)
            {
                return;
            }

            await RemoveExistingYearAsync(year);

            var academicPlans = selectedPlans
                .Select(x => new AcademicPlan
                {
                    Id = x.Id,
                    EducationDirectionId = 0,
                    AcademicCourses = default,
                    Year = x.Year
                })
                .ToList();

            await _context.AcademicPlans.AddRangeAsync(academicPlans);

            var academicPlanRecords = new List<AcademicPlanRecord>();

            foreach (var plan in selectedPlans)
            {
                var records = await LoadLegacyPlanRecordsAsync(plan.Id);

                academicPlanRecords.AddRange(records.Select(x => new AcademicPlanRecord
                {
                    AcademicPlanId = plan.Id,
                    DisciplineId = x.DisciplineId,
                    AcademicPlanRecordParentId = null,
                    InDepartment = false,
                    Semester = (Semester)x.Semester,
                    Zet = (int)x.Zet,
                    IsParent = false,
                    IsChild = false,
                    IsFacultative = false,
                    IsUseInWorkload = true,
                    IsActiveSemester = x.IsActiveSemester,
                    DisciplineBlockId = x.DisciplineBlockId
                }));
            }

            await _context.AcademicPlanRecords.AddRangeAsync(academicPlanRecords);
            await _context.SaveChangesAsync();
        }

        private async Task RemoveExistingYearAsync(string year)
        {
            var planIds = await _context.AcademicPlans
                .Where(x => x.Year == year)
                .Select(x => x.Id)
                .ToListAsync();

            if (planIds.Count == 0)
            {
                return;
            }

            var recordIds = await _context.AcademicPlanRecords
                .Where(x => planIds.Contains(x.AcademicPlanId))
                .Select(x => x.Id)
                .ToListAsync();

            if (recordIds.Count > 0)
            {
                var elements = await _context.AcademicPlanRecordElements
                    .Where(x => recordIds.Contains(x.AcademicPlanRecordId))
                    .ToListAsync();

                if (elements.Count > 0)
                {
                    _context.AcademicPlanRecordElements.RemoveRange(elements);
                }

                var records = await _context.AcademicPlanRecords
                    .Where(x => recordIds.Contains(x.Id))
                    .ToListAsync();

                if (records.Count > 0)
                {
                    _context.AcademicPlanRecords.RemoveRange(records);
                }
            }

            var plans = await _context.AcademicPlans
                .Where(x => planIds.Contains(x.Id))
                .ToListAsync();

            if (plans.Count > 0)
            {
                _context.AcademicPlans.RemoveRange(plans);
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpsertDisciplinesAsync(List<DisciplineImportModel> importedDisciplines)
        {
            if (importedDisciplines == null || importedDisciplines.Count == 0)
            {
                return;
            }

            var disciplineIds = importedDisciplines
                .Select(x => x.Id)
                .ToList();

            var existingIds = await _context.Disciplines
                .Where(x => disciplineIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var newDisciplines = importedDisciplines
                .Where(x => !existingIds.Contains(x.Id))
                .Select(x => new Discipline
                {
                    Id = x.Id,
                    DisciplineBlockId = x.DisciplineBlockId,
                    DisciplineName = x.DisciplineName,
                    DisciplineShortName = x.DisciplineShortName,
                    DisciplineDescription = x.DisciplineDescription,
                    DisciplineBlockBlueAsteriskName = x.DisciplineBlockBlueAsteriskName
                })
                .ToList();

            if (newDisciplines.Count > 0)
            {
                await _context.Disciplines.AddRangeAsync(newDisciplines);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<AcademicPlanImportFileModel?> LoadCombinedImportAsync()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "ImportData", "academic-plan-import.json");

            if (!File.Exists(filePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<AcademicPlanImportFileModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        private async Task<List<AcademicPlanModel>> LoadLegacyAcademicPlansAsync()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "ImportData", "academic-plan.json");

            if (!File.Exists(filePath))
            {
                return new List<AcademicPlanModel>();
            }

            var json = await File.ReadAllTextAsync(filePath);

            var data = JsonSerializer.Deserialize<AcademicPlanFileModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return data?.AcademicPlans ?? new List<AcademicPlanModel>();
        }

        private async Task<List<PlanRecordModel>> LoadLegacyPlanRecordsAsync(int academicPlanId)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "ImportData", "academic-plan-records.json");

            if (!File.Exists(filePath))
            {
                return new List<PlanRecordModel>();
            }

            var json = await File.ReadAllTextAsync(filePath);

            var data = JsonSerializer.Deserialize<AcademicPlanRecordFileModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return data?.PlanRecords?
                       .Where(x => x.AcademicPlanId == academicPlanId)
                       .ToList()
                   ?? new List<PlanRecordModel>();
        }

        private static string NormalizeAcademicYear(string year)
        {
            return string.IsNullOrWhiteSpace(year)
                ? string.Empty
                : year.Trim();
        }

        private static string ToAcademicYearString(int startYear)
        {
            return $"{startYear}-{startYear + 1}";
        }

        private static int ParseAcademicYearStart(string? academicYear)
        {
            if (string.IsNullOrWhiteSpace(academicYear))
            {
                return 0;
            }

            var parts = academicYear.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return 0;
            }

            return int.TryParse(parts[0], out var year)
                ? year
                : 0;
        }

        private static AcademicCourse MapAcademicCourse(int value)
        {
            return Enum.IsDefined(typeof(AcademicCourse), value)
                ? (AcademicCourse)value
                : default;
        }
    }
}