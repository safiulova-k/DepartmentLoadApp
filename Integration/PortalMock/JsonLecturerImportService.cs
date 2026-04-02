using System.Text.Json;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PortalMock.Models;
using DepartmentLoadApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.PortalMock
{
    public class JsonLecturerImportService : ILecturerImportService
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public JsonLecturerImportService(
            DepartmentLoadDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task ImportAsync()
        {
            var file = await LoadAsync();

            foreach (var lecturer in file.Lecturers)
            {
                var fullName = BuildFullName(lecturer);

                var dbItem = await _context.Teachers
                    .FirstOrDefaultAsync(x => x.ExternalLecturerId == lecturer.Id);

                if (dbItem == null)
                {
                    _context.Teachers.Add(new Teacher
                    {
                        ExternalLecturerId = lecturer.Id,
                        FullName = fullName,
                        Position = lecturer.Position
                    });
                }
                else
                {
                    dbItem.FullName = fullName;
                    dbItem.Position = lecturer.Position;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<LecturerImportFileModel> LoadAsync()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "MockData", "lecturers-mock.json");

            if (!File.Exists(filePath))
            {
                return new LecturerImportFileModel();
            }

            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<LecturerImportFileModel>(
                       json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new LecturerImportFileModel();
        }

        private static string BuildFullName(LecturerImportModel lecturer)
        {
            return string.Join(" ", new[]
            {
                lecturer.LastName?.Trim(),
                lecturer.FirstName?.Trim(),
                lecturer.Patronymic?.Trim()
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}