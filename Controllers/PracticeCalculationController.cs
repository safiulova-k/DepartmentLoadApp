using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.ViewModels.Practice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class PracticeCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;

        public PracticeCalculationController(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await _context.PracticeWorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            await RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return View(new PracticeWorkloadPageViewModel
            {
                SelectedYear = selectedYear,
                SelectedYearStart = selectedYearStart,
                AvailableYearStarts = AcademicYearResolver.BuildAvailableStartYears(selectedYearStart),
                Rows = rows
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromAcademicPlan(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var plans = await _context.AcademicPlansCore
                .AsNoTracking()
                .ToListAsync();

            var planIds = plans.Select(x => x.Id).ToList();

            var records = await _context.AcademicPlanRecordsCore
                .AsNoTracking()
                .Where(x => planIds.Contains(x.AcademicPlanId))
                .ToListAsync();

            var directions = await _context.EducationDirections
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id);

            var existingRows = await _context.PracticeWorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .ToListAsync();

            var weeksByRecordId = existingRows
                .Where(x => x.AcademicPlanRecordId > 0)
                .GroupBy(x => x.AcademicPlanRecordId)
                .ToDictionary(x => x.Key, x => x.First().WeeksCount);

            if (existingRows.Any())
            {
                _context.PracticeWorkloadRows.RemoveRange(existingRows);
                await _context.SaveChangesAsync();
            }

            var importedRows = new List<PracticeWorkloadRow>();

            foreach (var plan in plans)
            {
                if (plan.EducationDirectionId == null)
                    continue;

                if (!directions.TryGetValue(plan.EducationDirectionId.Value, out var direction))
                    continue;

                if (!AcademicYearResolver.TryResolveCourseAndSemesters(
                        plan.Year,
                        selectedYearStart,
                        out var course,
                        out var semesters))
                    continue;

                var planRecords = records
                    .Where(x => x.AcademicPlanId == plan.Id)
                    .Where(x => semesters.Contains(x.Semester))
                    .Where(x => IsPracticeRecord(x.Index))
                    .ToList();

                foreach (var record in planRecords)
                {
                    importedRows.Add(new PracticeWorkloadRow
                    {
                        PlanYear = selectedYear,
                        AcademicPlanId = plan.Id,
                        AcademicPlanRecordId = record.Id,

                        PracticeName = NormalizePracticeName(record.Name),
                        DirectionCode = direction.Cipher,
                        DirectionName = direction.Title,

                        Course = course,
                        SemesterName = AcademicYearResolver.GetSemesterName(record.Semester),
                        EducationForm = GetEducationFormName(plan),

                        WeeksCount = weeksByRecordId.TryGetValue(record.Id, out var weeks)
                            ? weeks
                            : 0
                    });
                }
            }

            if (importedRows.Any())
            {
                await _context.PracticeWorkloadRows.AddRangeAsync(importedRows);
                await _context.SaveChangesAsync();

                await RecalculateAsync(importedRows);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { startYear = selectedYearStart });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(PracticeWorkloadPageViewModel model)
        {
            var ids = model.Rows.Select(x => x.Id).ToList();

            var dbRows = await _context.PracticeWorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            foreach (var row in model.Rows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == row.Id);
                if (dbRow == null)
                    continue;

                dbRow.WeeksCount = row.WeeksCount;
            }

            await RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { startYear = model.SelectedYearStart });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await _context.PracticeWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            await RecalculateAsync(rows);

            var content = ExcelExportHelper.ExportPractice(rows);
            var fileName = $"Расчет_практик_{selectedYear}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task RecalculateAsync(List<PracticeWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x => x.CategoryName == "Практика" || x.CategoryName == "Научная работа")
                .ToListAsync();

            var contingentMap = await _context.ContingentRows
                .AsNoTracking()
                .ToDictionaryAsync(x => x.DirectionCode);

            foreach (var row in rows)
            {
                if (!contingentMap.TryGetValue(row.DirectionCode, out var contingent))
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.TotalHours = 0;
                    continue;
                }

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);

                var norm = norms.FirstOrDefault(x => x.WorkName == row.PracticeName);
                if (norm == null)
                {
                    row.TotalHours = 0;
                    continue;
                }

                var result = CalculationHelper.CalculateByNorm(
                    calculationBase: norm.CalculationBase,
                    coefficient: norm.Hours,
                    studentsCount: row.StudentsCount,
                    groupCount: row.GroupCount,
                    weeksCount: row.WeeksCount);

                row.TotalHours = CalculationHelper.RoundHours(result);
            }
        }

        private static bool IsPracticeRecord(string? index)
        {
            if (string.IsNullOrWhiteSpace(index))
                return false;

            var normalized = index.Trim().ToUpperInvariant();
            return normalized.StartsWith("Б2");
        }

        private static string NormalizePracticeName(string? sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                return string.Empty;

            var value = sourceName.Trim().ToLowerInvariant();

            if (value.Contains("ознаком"))
                return "Ознакомительная практика";

            if (value.Contains("технолог") || value.Contains("производствен"))
                return "Технологическая практика";

            if (value.Contains("преддиплом") && value.Contains("магистр"))
                return "Преддипломная практика магистров";

            if (value.Contains("преддиплом"))
                return "Преддипломная практика бакалавров";

            if (value.Contains("нирм"))
                return "НИРМ";

            if (value.Contains("научно-исследовательская"))
                return "Научно-исследовательская работа";

            if (value == "нир" || value.Contains(" нир"))
                return "НИР";

            if (value.Contains("учебн"))
                return "Учебная практика";

            return sourceName.Trim();
        }

        private static string GetEducationFormName(DepartmentLoadApp.Models.Core.AcademicPlan plan)
        {
            return plan.EducationForm.ToString();
        }
    }
}