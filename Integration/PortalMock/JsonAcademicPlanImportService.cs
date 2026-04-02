using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PortalMock;
using DepartmentLoadApp.Integration.PortalMock.Models;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.AcademicPlan;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DepartmentLoadApp.Integration
{
    public class JsonAcademicPlanImportService : IAcademicPlanImportService
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public JsonAcademicPlanImportService(DepartmentLoadDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Импортирует данные учебного года
        public async Task ImportYearAsync(string year)
        {
            var academicPlans = await LoadAcademicPlansAsync(year);

            if (academicPlans == null || academicPlans.Count == 0)
            {
                return; // Если данных нет, просто выходим
            }

            var existingPlans = await _context.AcademicPlans
                .Where(x => x.Year == year)
                .ToListAsync();

            _context.AcademicPlans.RemoveRange(existingPlans);

            var newAcademicPlans = academicPlans.Select(plan => new AcademicPlan
            {
                Year = plan.Year,
                Id = plan.Id
            }).ToList();

            _context.AcademicPlans.AddRange(newAcademicPlans);

            var newPlanRecords = new List<AcademicPlanRecord>();

            foreach (var plan in newAcademicPlans)
            {
                var records = await LoadPlanRecordsAsync(plan.Id);
                foreach (var record in records)
                {
                    newPlanRecords.Add(new AcademicPlanRecord
                    {
                        AcademicPlanId = plan.Id,
                        DisciplineId = record.DisciplineId,
                        Semester = (Models.Enums.Semester)record.Semester,
                        Zet = (int)record.Zet,
                        IsActiveSemester = record.IsActiveSemester,
                        DisciplineBlockId = record.DisciplineBlockId
                    });
                }
            }

            _context.AcademicPlanRecords.AddRange(newPlanRecords);

            await _context.SaveChangesAsync();
        }

        // Загружает учебные планы из файла JSON
        private async Task<List<AcademicPlanModel>> LoadAcademicPlansAsync(string year)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "MockData", "academic-plan-mock.json");

            if (!File.Exists(filePath))
            {
                return new List<AcademicPlanModel>(); // Возвращаем пустой список, если файл не существует
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<AcademicPlanFileModel>(json);

            if (data == null || data.AcademicPlans == null)
            {
                return new List<AcademicPlanModel>(); // Возвращаем пустой список, если данных нет
            }

            // Возвращаем только учебные планы для указанного года
            return data.AcademicPlans
                .Where(x => x.Year == year)
                .ToList();
        }

        // Загружает записи для учебного плана из файла JSON
        private async Task<List<PlanRecordModel>> LoadPlanRecordsAsync(int academicPlanId)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "MockData", "academic-plan-records-mock.json");

            if (!File.Exists(filePath))
            {
                return new List<PlanRecordModel>();
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<AcademicPlanRecordFileModel>(json);

            // Возвращаем только записи для указанного учебного плана
            return data?.PlanRecords
                .Where(x => x.AcademicPlanId == academicPlanId)
                .ToList() ?? new List<PlanRecordModel>();
        }

        // Получение последнего учебного года из файлов
        public async Task<int?> GetLatestYearAsync()
        {
            var academicPlans = await LoadAcademicPlansAsync("2025-2026");

            if (academicPlans == null || academicPlans.Count == 0)
            {
                return null;
            }

            // Предположим, что мы всегда будем возвращать последний учебный год
            var latestYear = academicPlans.Max(x => x.Year);

            return int.Parse(latestYear.Split('-')[0]); // Получаем только первый год
        }

        // Проверяет, был ли импортирован учебный год, и если нет, выполняет импорт
        public async Task EnsureYearImportedAsync(string year)
        {
            var isYearImported = await _context.AcademicPlans
                .AnyAsync(x => x.Year == year);

            if (!isYearImported)
            {
                await ImportYearAsync(year);
            }
        }
    }
}