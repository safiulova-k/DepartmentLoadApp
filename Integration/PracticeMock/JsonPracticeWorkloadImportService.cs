using System.Text.Json;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PracticeMock.Models;
using DepartmentLoadApp.Models.Practice;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.PracticeMock
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

        public async Task EnsureYearImportedAsync(int year)
        {
            var hasRows = await _context.PracticeWorkloadRows
                .AnyAsync(x => x.PlanYear == year);

            if (hasRows)
            {
                return;
            }

            var data = await LoadAsync();

            var rows = data.Rows
                .Where(x => x.PlanYear == year)
                .Select(x => new PracticeWorkloadRow
                {
                    PlanYear = x.PlanYear,
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
            var filePath = Path.Combine(_environment.ContentRootPath, "MockData", "practice-workload-mock.json");

            if (!File.Exists(filePath))
            {
                return new PracticeWorkloadImportFileModel();
            }

            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<PracticeWorkloadImportFileModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new PracticeWorkloadImportFileModel();
        }
        private static string GetSemesterName(int semesterNumber)
        {
            if (semesterNumber <= 0)
                return string.Empty;

            return semesterNumber % 2 == 0 ? "весна" : "осень";
        }
    }
}