using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
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
        private readonly WorkloadCalculationService _workloadCalculationService;
        private readonly PracticeCalculationService _practiceCalculationService;
        private readonly GiaCalculationService _giaCalculationService;

        public WorkloadCalculationController(
            DepartmentLoadDbContext context,
            CalculationImportService importService,
            WorkloadCalculationService workloadCalculationService,
            PracticeCalculationService practiceCalculationService,
            GiaCalculationService giaCalculationService)
        {
            _context = context;
            _importService = importService;
            _workloadCalculationService = workloadCalculationService;
            _practiceCalculationService = practiceCalculationService;
            _giaCalculationService = giaCalculationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var rows = await LoadWorkloadRowsAsync(selectedYear, asNoTracking: true);

            await _workloadCalculationService.RecalculateAsync(rows);

            var model = new WorkloadTablePageViewModel
            {
                SelectedYearStart = selectedYearStart,
                SelectedYear = selectedYear,
                AvailableYearStarts = AcademicYearResolver.BuildAvailableStartYears(selectedYearStart),
                Rows = rows
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromAcademicPlan(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);

            await _importService.ImportAllAsync(selectedYearStart);

            TempData["SuccessMessage"] = "Данные из учебного плана импортированы";

            return RedirectToIndex(selectedYearStart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(WorkloadTablePageViewModel model)
        {
            var inputRows = model.Rows ?? new List<WorkloadRow>();

            if (!inputRows.Any())
            {
                TempData["ErrorMessage"] = "Нет строк для сохранения";

                return RedirectToIndex(model.SelectedYearStart);
            }

            var ids = inputRows
                .Select(x => x.Id)
                .ToList();

            var dbRows = await _context.WorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            foreach (var inputRow in inputRows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == inputRow.Id);

                if (dbRow == null)
                {
                    continue;
                }

                dbRow.LecturePlanHours = inputRow.LecturePlanHours;
                dbRow.PracticePlanHours = inputRow.PracticePlanHours;
                dbRow.LabPlanHours = inputRow.LabPlanHours;

                dbRow.HasExam = inputRow.HasExam;
                dbRow.HasCredit = inputRow.HasCredit;
                dbRow.HasCourseWork = inputRow.HasCourseWork;
                dbRow.HasCourseProject = inputRow.HasCourseProject;
                dbRow.HasRgr = inputRow.HasRgr;
            }

            await _workloadCalculationService.RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Расчёт нагрузки сохранён";

            return RedirectToIndex(model.SelectedYearStart);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var workloadRows = await LoadWorkloadRowsAsync(selectedYear, asNoTracking: true);
            await _workloadCalculationService.RecalculateAsync(workloadRows);

            var practiceRows = await LoadPracticeRowsAsync(selectedYear);
            await _practiceCalculationService.RecalculateAsync(practiceRows);

            var giaRows = await LoadGiaRowsAsync(selectedYear);
            await _giaCalculationService.RecalculateAsync(giaRows);

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

        private RedirectToActionResult RedirectToIndex(int startYear)
        {
            return RedirectToAction(nameof(Index), new
            {
                startYear
            });
        }

        private async Task<List<WorkloadRow>> LoadWorkloadRowsAsync(
            string selectedYear,
            bool asNoTracking)
        {
            var query = _context.WorkloadRows
                .Where(x => x.AcademicYear == selectedYear);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();
        }

        private async Task<List<PracticeWorkloadRow>> LoadPracticeRowsAsync(
            string selectedYear)
        {
            return await _context.PracticeWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();
        }

        private async Task<List<GiaWorkloadRow>> LoadGiaRowsAsync(
            string selectedYear)
        {
            return await _context.GiaWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.DirectionCode)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();
        }
    }
}