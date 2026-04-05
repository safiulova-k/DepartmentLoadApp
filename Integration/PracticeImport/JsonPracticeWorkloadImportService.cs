using System.Text.Json;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PracticeImport.Models;
using DepartmentLoadApp.Models.Practice;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.PracticeImport
{
    public class JsonPracticeWorkloadImportService : IPracticeWorkloadImportService
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public JsonPracticeWorkloadImportService(
            DepartmentLoadDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task EnsureYearImportedAsync(string year)
        {
            var normalizedYear = NormalizeAcademicYear(year);

            var hasRows = await _context.PracticeWorkloadRows
                .AnyAsync(x => x.PlanYear == normalizedYear);

            if (hasRows)
            {
                return;
            }

            var data = await LoadAsync();

            var rows = data.Rows
                .Where(x => ToAcademicYearString(x.PlanYear) == normalizedYear)
                .Select(x => new PracticeWorkloadRow
                {
                    PlanYear = ToAcademicYearString(x.PlanYear),
                    PracticeName = x.PracticeName,
                    DirectionCode = x.DirectionCode,
                    DirectionName = x.DirectionName,
                    Course = x.Course,
                    SemesterName = GetSemesterName(x.Semester),
                    EducationForm = x.EducationForm,
                    WeeksCount = x.WeeksCount,
                    StudentsCount = 0,
                    GroupCount = 0,
                    TotalHours = 0
                })
                .ToList();

            if (rows.Count == 0)
            {
                return;
            }

            await _context.PracticeWorkloadRows.AddRangeAsync(rows);
            await _context.SaveChangesAsync();
        }

        private async Task<PracticeWorkloadImportFileModel> LoadAsync()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "ImportData", "practice-workload.json");

            if (!File.Exists(filePath))
            {
                return new PracticeWorkloadImportFileModel();
            }

            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<PracticeWorkloadImportFileModel>(
                       json,
                       new JsonSerializerOptions
                       {
                           PropertyNameCaseInsensitive = true
                       })
                   ?? new PracticeWorkloadImportFileModel();
        }

        private static string NormalizeAcademicYear(string year)
        {
            return string.IsNullOrWhiteSpace(year)
                ? string.Empty
                : year.Trim();
        }

        private static string GetSemesterName(int semesterNumber)
        {
            if (semesterNumber <= 0)
            {
                return string.Empty;
            }

            return semesterNumber % 2 == 0 ? "весна" : "осень";
        }
        private static string ToAcademicYearString(int startYear)
        {
            return $"{startYear}-{startYear + 1}";
        }
    }
}