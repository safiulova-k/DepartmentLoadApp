using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.ViewModels.Gia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DepartmentLoadApp.Services;

namespace DepartmentLoadApp.Controllers
{
    public class GiaCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly CalculationImportService _importService;

        public GiaCalculationController(
            DepartmentLoadDbContext context,
            CalculationImportService importService)
        {
            _context = context;
            _importService = importService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await _context.GiaWorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            await RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return View(new GiaWorkloadPageViewModel
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

            await _importService.ImportAllAsync(selectedYearStart);

            return RedirectToAction(nameof(Index), new { startYear = selectedYearStart });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(GiaWorkloadPageViewModel model)
        {
            var ids = model.Rows.Select(x => x.Id).ToList();

            var dbRows = await _context.GiaWorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            foreach (var row in model.Rows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == row.Id);
                if (dbRow == null)
                    continue;

                if (dbRow.WorkName == "Консультация к госэкзамену")
                {
                    dbRow.ManualHours = row.ManualHours;
                }
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

            var rows = await _context.GiaWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            await RecalculateAsync(rows);

            var content = ExcelExportHelper.ExportGia(rows);
            var fileName = $"Расчет_ГИА_{selectedYear}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task RecalculateAsync(List<GiaWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x => x.CategoryName == "ГИА")
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

                row.TotalHours = CalculateGiaHours(row, norms, contingent);
            }
        }

        private decimal CalculateGiaHours(
            GiaWorkloadRow row,
            List<NormTime> norms,
            ContingentRow contingent)
        {
            if (row.WorkName == "Консультация к госэкзамену")
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

        private static string GetGiaNormName(GiaWorkloadRow row, ContingentRow contingent)
        {
            if (row.WorkName == "Руководство ВКР")
            {
                return contingent.IsMaster
                    ? "Руководство ВКР магистра"
                    : "Руководство ВКР бакалавра";
            }

            return row.WorkName;
        }

        private static bool IsGiaRecord(string? index)
        {
            if (string.IsNullOrWhiteSpace(index))
                return false;

            var normalized = index.Trim().ToUpperInvariant();
            return normalized.StartsWith("Б3");
        }

        private static bool IsStateExamRecord(DepartmentLoadApp.Models.Core.AcademicPlanRecord record)
        {
            if ((record.Exam ?? 0) > 0)
                return true;

            var name = (record.Name ?? string.Empty).ToLowerInvariant();

            return name.Contains("государствен") && name.Contains("экзам");
        }

        private static void AddGiaRow(
            List<GiaWorkloadRow> rows,
            string planYear,
            int academicPlanId,
            int academicPlanRecordId,
            string giaSection,
            string workName,
            string directionCode,
            string directionName,
            int course,
            string semesterName,
            string educationForm,
            Dictionary<string, decimal> manualHoursMap)
        {
            var key = BuildGiaManualKey(academicPlanRecordId, workName);

            rows.Add(new GiaWorkloadRow
            {
                PlanYear = planYear,
                AcademicPlanId = academicPlanId,
                AcademicPlanRecordId = academicPlanRecordId,
                GiaSection = giaSection,
                WorkName = workName,
                DirectionCode = directionCode,
                DirectionName = directionName,
                Course = course,
                SemesterName = semesterName,
                EducationForm = educationForm,
                ManualHours = manualHoursMap.TryGetValue(key, out var manualHours)
                    ? manualHours
                    : 0
            });
        }

        private static string BuildGiaManualKey(int recordId, string workName)
        {
            return $"{recordId}|{workName}";
        }

        private static string GetEducationFormName(DepartmentLoadApp.Models.Core.AcademicPlan plan)
        {
            return plan.EducationForm.ToString();
        }
    }
}