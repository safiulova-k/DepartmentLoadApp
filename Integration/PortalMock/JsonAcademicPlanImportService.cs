using System.Text.Json;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PortalMock.Models;
using DepartmentLoadApp.Models.AcademicPlan;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Workload;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.PortalMock
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

        public async Task<int?> GetLatestYearAsync()
        {
            var data = await LoadAsync();

            if (data.AcademicPlans.Count == 0)
            {
                return null;
            }

            return data.AcademicPlans.Max(x => x.Year);
        }

        public async Task EnsureYearImportedAsync(int year)
        {
            var hasRows = await _context.WorkloadRows.AnyAsync(x => x.PlanYear == year);

            if (!hasRows)
            {
                await ImportYearAsync(year);
            }
        }

        public async Task ImportYearAsync(int year)
        {
            var data = await LoadAsync();

            var importedPlans = data.AcademicPlans
                .Where(x => x.Year == year)
                .ToList();

            if (importedPlans.Count == 0)
            {
                return;
            }

            var importedPlanIds = importedPlans
                .Select(x => x.Id)
                .ToHashSet();

            var importedRecords = data.AcademicPlanRecords
                .Where(x => importedPlanIds.Contains(x.AcademicPlanId))
                .ToList();

            var importedRecordIds = importedRecords
                .Select(x => x.Id)
                .ToHashSet();

            var importedDisciplineIds = importedRecords
                .Select(x => x.DisciplineId)
                .Distinct()
                .ToHashSet();

            var importedDisciplines = data.Disciplines
                .Where(x => importedDisciplineIds.Contains(x.Id))
                .ToList();

            var importedElements = data.AcademicPlanRecordElements
                .Where(x => importedRecordIds.Contains(x.AcademicPlanRecordId))
                .ToList();

            await RemoveExistingYearAsync(year);

            await UpsertDisciplinesAsync(importedDisciplines);
            await InsertPlansAsync(importedPlans);
            await InsertRecordsAsync(importedRecords);
            await InsertElementsAsync(importedElements);
            await BuildWorkloadRowsAsync(importedPlans, importedRecords, importedDisciplines, importedElements);
        }

        private async Task RemoveExistingYearAsync(int year)
        {
            var existingRows = await _context.WorkloadRows
                .Where(x => x.PlanYear == year)
                .ToListAsync();

            if (existingRows.Count > 0)
            {
                _context.WorkloadRows.RemoveRange(existingRows);
            }

            var existingPlanIds = await _context.AcademicPlans
                .Where(x => x.Year == year)
                .Select(x => x.Id)
                .ToListAsync();

            if (existingPlanIds.Count > 0)
            {
                var existingRecordIds = await _context.AcademicPlanRecords
                    .Where(x => existingPlanIds.Contains(x.AcademicPlanId))
                    .Select(x => x.Id)
                    .ToListAsync();

                if (existingRecordIds.Count > 0)
                {
                    var existingElements = await _context.AcademicPlanRecordElements
                        .Where(x => existingRecordIds.Contains(x.AcademicPlanRecordId))
                        .ToListAsync();

                    if (existingElements.Count > 0)
                    {
                        _context.AcademicPlanRecordElements.RemoveRange(existingElements);
                    }

                    var existingRecords = await _context.AcademicPlanRecords
                        .Where(x => existingRecordIds.Contains(x.Id))
                        .ToListAsync();

                    if (existingRecords.Count > 0)
                    {
                        _context.AcademicPlanRecords.RemoveRange(existingRecords);
                    }
                }

                var existingPlans = await _context.AcademicPlans
                    .Where(x => existingPlanIds.Contains(x.Id))
                    .ToListAsync();

                if (existingPlans.Count > 0)
                {
                    _context.AcademicPlans.RemoveRange(existingPlans);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpsertDisciplinesAsync(List<DisciplineImportModel> disciplines)
        {
            foreach (var item in disciplines)
            {
                var dbItem = await _context.Disciplines
                    .FirstOrDefaultAsync(x => x.Id == item.Id);

                if (dbItem == null)
                {
                    _context.Disciplines.Add(new Discipline
                    {
                        Id = item.Id,
                        DisciplineBlockId = item.DisciplineBlockId,
                        DisciplineName = item.DisciplineName,
                        DisciplineShortName = item.DisciplineShortName,
                        DisciplineDescription = item.DisciplineDescription,
                        DisciplineBlockBlueAsteriskName = item.DisciplineBlockBlueAsteriskName
                    });
                }
                else
                {
                    dbItem.DisciplineBlockId = item.DisciplineBlockId;
                    dbItem.DisciplineName = item.DisciplineName;
                    dbItem.DisciplineShortName = item.DisciplineShortName;
                    dbItem.DisciplineDescription = item.DisciplineDescription;
                    dbItem.DisciplineBlockBlueAsteriskName = item.DisciplineBlockBlueAsteriskName;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task InsertPlansAsync(List<AcademicPlanImportModel> plans)
        {
            foreach (var item in plans)
            {
                _context.AcademicPlans.Add(new AcademicPlan
                {
                    Id = item.Id,
                    EducationDirectionId = item.EducationDirectionId,
                    AcademicCourses = (AcademicCourse)item.AcademicCourses,
                    Year = item.Year
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task InsertRecordsAsync(List<AcademicPlanRecordImportModel> records)
        {
            foreach (var item in records)
            {
                _context.AcademicPlanRecords.Add(new AcademicPlanRecord
                {
                    Id = item.Id,
                    AcademicPlanId = item.AcademicPlanId,
                    DisciplineId = item.DisciplineId,
                    AcademicPlanRecordParentId = item.AcademicPlanRecordParentId,
                    InDepartment = item.InDepartment,
                    Semester = (Semester)item.Semester,
                    Zet = item.Zet,
                    IsParent = item.IsParent,
                    IsChild = item.IsChild,
                    IsFacultative = item.IsFacultative,
                    IsUseInWorkload = item.IsUseInWorkload,
                    IsActiveSemester = item.IsActiveSemester
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task InsertElementsAsync(List<AcademicPlanRecordElementImportModel> elements)
        {
            foreach (var item in elements)
            {
                _context.AcademicPlanRecordElements.Add(new AcademicPlanRecordElement
                {
                    Id = item.Id,
                    AcademicPlanRecordId = item.AcademicPlanRecordId,
                    ActivityType = item.ActivityType,
                    PlanHours = item.PlanHours,
                    FactHours = item.FactHours
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task BuildWorkloadRowsAsync(
            List<AcademicPlanImportModel> plans,
            List<AcademicPlanRecordImportModel> records,
            List<DisciplineImportModel> disciplines,
            List<AcademicPlanRecordElementImportModel> elements)
        {
            var disciplineMap = disciplines.ToDictionary(x => x.Id, x => x);
            var planMap = plans.ToDictionary(x => x.Id, x => x);

            var workloadRecords = records
                .Where(x => x.IsUseInWorkload)
                .Where(x => x.IsActiveSemester)
                .Where(x => !x.IsParent)
                .ToList();

            var rows = new List<WorkloadRow>();

            foreach (var record in workloadRecords)
            {
                if (!planMap.TryGetValue(record.AcademicPlanId, out var plan))
                {
                    continue;
                }

                if (!disciplineMap.TryGetValue(record.DisciplineId, out var discipline))
                {
                    continue;
                }

                var recordElements = elements
                    .Where(x => x.AcademicPlanRecordId == record.Id)
                    .ToList();

                var lecturePlanHours = recordElements
                    .Where(x => x.ActivityType == "Лекции")
                    .Sum(x => x.PlanHours);

                var practicePlanHours = recordElements
                    .Where(x => x.ActivityType == "Практические занятия")
                    .Sum(x => x.PlanHours);

                var labPlanHours = recordElements
                    .Where(x => x.ActivityType == "Лабораторные работы")
                    .Sum(x => x.PlanHours);

                rows.Add(new WorkloadRow
                {
                    PlanYear = plan.Year,
                    AcademicPlanId = plan.Id,
                    AcademicPlanRecordId = record.Id,
                    DisciplineId = discipline.Id,
                    DisciplineName = discipline.DisciplineName,

                    DirectionCode = FormatDirectionCode(plan.EducationDirectionId),
                    DirectionName = $"Направление {FormatDirectionCode(plan.EducationDirectionId)}",
                    SemesterName = ((int)record.Semester).ToString(),
                    EducationForm = "Очная",
                    Course = (int)plan.AcademicCourses,

                    StudentsCount = 0,
                    FlowCount = 1,
                    GroupCount = 0,
                    SubgroupCount = 0,

                    LecturePlanHours = lecturePlanHours,
                    LectureTotalHours = 0,

                    PracticePlanHours = practicePlanHours,
                    PracticeTotalHours = 0,

                    LabPlanHours = labPlanHours,
                    LabTotalHours = 0
                });
            }

            if (rows.Count > 0)
            {
                await _context.WorkloadRows.AddRangeAsync(rows);
                await _context.SaveChangesAsync();
            }
        }

        private static string FormatDirectionCode(int educationDirectionId)
        {
            var value = educationDirectionId.ToString().PadLeft(6, '0');

            return $"{value.Substring(0, 2)}.{value.Substring(2, 2)}.{value.Substring(4, 2)}";
        }

        private async Task<AcademicPlanImportFileModel> LoadAsync()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "MockData", "academic-plan-mock.json");

            if (!File.Exists(filePath))
            {
                return new AcademicPlanImportFileModel();
            }

            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<AcademicPlanImportFileModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AcademicPlanImportFileModel();
        }
    }
}