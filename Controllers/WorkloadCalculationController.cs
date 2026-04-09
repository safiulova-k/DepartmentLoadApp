using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.Services;
using DepartmentLoadApp.ViewModels.Workload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly CalculationImportService _importService;

        public WorkloadCalculationController(
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

            var rows = await LoadRowsAsync(selectedYear);
            await RecalculateAsync(rows);
            await _context.SaveChangesAsync();

            return View(new WorkloadTablePageViewModel
            {
                SelectedYearStart = selectedYearStart,
                SelectedYear = selectedYear,
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
        public async Task<IActionResult> Save(WorkloadTablePageViewModel model)
        {
            var ids = model.Rows.Select(x => x.Id).ToList();

            var dbRows = await _context.WorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            await RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { startYear = model.SelectedYearStart });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var workloadRows = await LoadRowsAsync(selectedYear);
            await RecalculateAsync(workloadRows);

            var practiceRows = await _context.PracticeWorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            await RecalculatePracticeAsync(practiceRows);

            var giaRows = await _context.GiaWorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            await RecalculateGiaAsync(giaRows);

            var content = ExcelExportHelper.ExportCombinedCalculation(
                selectedYear,
                workloadRows,
                practiceRows,
                giaRows);

            var fileName = $"Расчет_нагрузки_кафедры_{selectedYear}.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task<List<WorkloadRow>> LoadRowsAsync(string year)
        {
            return await _context.WorkloadRows
                .Where(x => x.AcademicYear == year)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();
        }

        private async Task RecalculateAsync(List<WorkloadRow> rows)
        {
            var lectureNorm = await GetNormAsync("Лекции");
            var practiceNorm = await GetNormAsync("Практические занятия");
            var labNorm = await GetNormAsync("Лабораторные работы");
            var consultationNorm = await GetNormAsync("Консультации");
            var consultationExamExtraNorm = await GetNormAsync("Доп. консультация к экзамену");
            var examNorm = await GetNormAsync("Экзамен");
            var creditNorm = await GetNormAsync("Зачет");
            var courseWorkNorm = await GetNormAsync("Курсовая работа");
            var courseProjectNorm = await GetNormAsync("Курсовой проект");
            var rgrNorm = await GetNormAsync("РГР");

            var contingentMap = await _context.ContingentRows
                .AsNoTracking()
                .ToDictionaryAsync(x => x.DirectionCode);

            var flows = await _context.StudentFlows
                .AsNoTracking()
                .ToListAsync();

            foreach (var row in rows)
            {
                if (!contingentMap.TryGetValue(row.DirectionCode, out var contingent))
                {
                    ResetCalculatedFields(row);
                    continue;
                }

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);
                row.SubgroupCount = CalculationHelper.GetSubgroupsByCourse(contingent, row.Course);

                row.FlowCount = flows.Count(x =>
                    x.AcademicYear == row.AcademicYear &&
                    x.DirectionCode == row.DirectionCode &&
                    x.Course == row.Course);

                if (row.FlowCount <= 0)
                {
                    row.FlowCount = row.GroupCount > 0 ? 1 : 0;
                }

                row.LectureTotalHours = NormCalculationHelper.CalculatePlanHours(row.LecturePlanHours, lectureNorm, row);
                row.PracticeTotalHours = NormCalculationHelper.CalculatePlanHours(row.PracticePlanHours, practiceNorm, row);
                row.LabTotalHours = NormCalculationHelper.CalculatePlanHours(row.LabPlanHours, labNorm, row);

                row.ExamHours = NormCalculationHelper.CalculateOptionalHours(row.HasExam, examNorm, row);
                row.CreditHours = NormCalculationHelper.CalculateOptionalHours(row.HasCredit, creditNorm, row);
                row.CourseWorkHours = NormCalculationHelper.CalculateOptionalHours(row.HasCourseWork, courseWorkNorm, row);
                row.CourseProjectHours = NormCalculationHelper.CalculateOptionalHours(row.HasCourseProject, courseProjectNorm, row);
                row.RgrHours = NormCalculationHelper.CalculateOptionalHours(row.HasRgr, rgrNorm, row);

                row.ConsultationHours = NormCalculationHelper.CalculateConsultationHours(
                    row,
                    consultationNorm,
                    consultationExamExtraNorm);
            }
        }

        private async Task RecalculatePracticeAsync(List<PracticeWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.CategoryName) &&
                    (EF.Functions.ILike(x.CategoryName, "%практи%") ||
                     EF.Functions.ILike(x.CategoryName, "%науч%")))
                .ToListAsync();

            var contingentMap = await _context.ContingentRows
                .AsNoTracking()
                .ToDictionaryAsync(x => NormalizeText(x.DirectionCode));

            foreach (var row in rows)
            {
                if (!contingentMap.TryGetValue(NormalizeText(row.DirectionCode), out var contingent))
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.TotalHours = 0;
                    continue;
                }

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);

                var norm = FindPracticeNorm(norms, row.PracticeName);

                if (norm == null || row.WeeksCount <= 0 || norm.Hours <= 0)
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

        private async Task RecalculateGiaAsync(List<GiaWorkloadRow> rows)
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

        private static NormTime? FindPracticeNorm(List<NormTime> norms, string practiceName)
        {
            var target = NormalizePracticeKey(practiceName);

            return norms.FirstOrDefault(x => NormalizePracticeKey(x.WorkName) == target)
                   ?? norms.FirstOrDefault(x => IsSamePracticeType(x.WorkName, practiceName));
        }

        private static bool IsSamePracticeType(string? left, string? right)
        {
            var a = NormalizePracticeKey(left);
            var b = NormalizePracticeKey(right);

            if (a == b) return true;
            if (a.Contains("технологическ") && b.Contains("технологическ")) return true;
            if (a.Contains("преддиплом") && b.Contains("преддиплом")) return true;
            if (a.Contains("ознаком") && b.Contains("ознаком")) return true;
            if (a == "нир" && b == "нир") return true;
            if (a.Contains("научно-исследователь") && b.Contains("научно-исследователь")) return true;
            if (a.Contains("учебн") && b.Contains("учебн")) return true;

            return false;
        }

        private static string NormalizePracticeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Replace("бакалавров", "")
                .Replace("магистров", "")
                .Replace("(учебная)", "")
                .Replace("(производственная)", "");

            return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(' ', value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static void ResetCalculatedFields(WorkloadRow row)
        {
            row.StudentsCount = 0;
            row.FlowCount = 0;
            row.GroupCount = 0;
            row.SubgroupCount = 0;
            row.LectureTotalHours = 0;
            row.PracticeTotalHours = 0;
            row.LabTotalHours = 0;
            row.ConsultationHours = 0;
            row.ExamHours = 0;
            row.CreditHours = 0;
            row.CourseWorkHours = 0;
            row.CourseProjectHours = 0;
            row.RgrHours = 0;
        }

        private Task<NormTime?> GetNormAsync(string workName)
        {
            return _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);
        }
    }
}