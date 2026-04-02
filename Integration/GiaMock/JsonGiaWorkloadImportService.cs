using System.Text.Json;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.GiaMock.Models;
using DepartmentLoadApp.Models.Gia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.GiaMock
{
    public class JsonGiaWorkloadImportService : IGiaWorkloadImportService
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public JsonGiaWorkloadImportService(
            DepartmentLoadDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task EnsureYearImportedAsync(int year)
        {
            var hasRows = await _context.GiaWorkloadRows.AnyAsync(x => x.PlanYear == year);
            if (hasRows)
            {
                return;
            }

            var data = await LoadAsync();

            var rows = new List<GiaWorkloadRow>();

            foreach (var item in data.Rows.Where(x => x.PlanYear == year))
            {
                if (item.HasStateExam)
                {
                    rows.Add(new GiaWorkloadRow
                    {
                        PlanYear = item.PlanYear,
                        GiaSection = "Госэкзамен",
                        WorkName = "Консультация к госэкзамену",
                        DirectionCode = item.DirectionCode,
                        DirectionName = item.DirectionName,
                        Course = item.Course,
                        SemesterName = GetSemesterName(item.Semester),
                        EducationForm = item.EducationForm,
                        StudentsCount = 0,
                        GroupCount = 0,
                        ManualHours = item.StateExamConsultationHours,
                        TotalHours = 0
                    });

                    rows.Add(new GiaWorkloadRow
                    {
                        PlanYear = item.PlanYear,
                        GiaSection = "Госэкзамен",
                        WorkName = "Госэкзамен",
                        DirectionCode = item.DirectionCode,
                        DirectionName = item.DirectionName,
                        Course = item.Course,
                        SemesterName = GetSemesterName(item.Semester),
                        EducationForm = item.EducationForm,
                        StudentsCount = 0,
                        GroupCount = 0,
                        ManualHours = 0,
                        TotalHours = 0
                    });
                }

                if (item.HasVkr)
                {
                    rows.Add(new GiaWorkloadRow
                    {
                        PlanYear = item.PlanYear,
                        GiaSection = "Дипломное проектирование",
                        WorkName = "Руководство ВКР",
                        DirectionCode = item.DirectionCode,
                        DirectionName = item.DirectionName,
                        Course = item.Course,
                        SemesterName = GetSemesterName(item.Semester),
                        EducationForm = item.EducationForm,
                        StudentsCount = 0,
                        GroupCount = 0,
                        ManualHours = 0,
                        TotalHours = 0
                    });

                    rows.Add(new GiaWorkloadRow
                    {
                        PlanYear = item.PlanYear,
                        GiaSection = "Дипломное проектирование",
                        WorkName = "Нормоконтроль ВКР",
                        DirectionCode = item.DirectionCode,
                        DirectionName = item.DirectionName,
                        Course = item.Course,
                        SemesterName = GetSemesterName(item.Semester),
                        EducationForm = item.EducationForm,
                        StudentsCount = 0,
                        GroupCount = 0,
                        ManualHours = 0,
                        TotalHours = 0
                    });

                    rows.Add(new GiaWorkloadRow
                    {
                        PlanYear = item.PlanYear,
                        GiaSection = "ГЭК",
                        WorkName = "Работа в ГЭК",
                        DirectionCode = item.DirectionCode,
                        DirectionName = item.DirectionName,
                        Course = item.Course,
                        SemesterName = GetSemesterName(item.Semester),
                        EducationForm = item.EducationForm,
                        StudentsCount = 0,
                        GroupCount = 0,
                        ManualHours = 0,
                        TotalHours = 0
                    });
                }
            }

            if (rows.Count == 0)
            {
                return;
            }

            await _context.GiaWorkloadRows.AddRangeAsync(rows);
            await _context.SaveChangesAsync();
        }

        private async Task<GiaWorkloadImportFileModel> LoadAsync()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "MockData", "gia-workload-mock.json");

            if (!File.Exists(filePath))
            {
                return new GiaWorkloadImportFileModel();
            }

            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<GiaWorkloadImportFileModel>(
                       json,
                       new JsonSerializerOptions
                       {
                           PropertyNameCaseInsensitive = true
                       })
                   ?? new GiaWorkloadImportFileModel();
        }
        private static string GetSemesterName(int semesterNumber)
        {
            if (semesterNumber <= 0)
                return string.Empty;

            return semesterNumber % 2 == 0 ? "весна" : "осень";
        }
    }
}