using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PortalMock;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.ViewModels.Workload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IAcademicPlanImportService _academicPlanImportService;

        public WorkloadCalculationController(
            DepartmentLoadDbContext context,
            IAcademicPlanImportService academicPlanImportService)
        {
            _context = context;
            _academicPlanImportService = academicPlanImportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? year)
        {
            var selectedYear = year
                               ?? await _academicPlanImportService.GetLatestYearAsync()
                               ?? DateTime.Now.Year;

            await _academicPlanImportService.EnsureYearImportedAsync(selectedYear);

            var rows = await _context.WorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            await Recalculate(rows);
            await _context.SaveChangesAsync();

            return View(new WorkloadTablePageViewModel
            {
                SelectedYear = selectedYear,
                Rows = rows
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(WorkloadTablePageViewModel model)
        {
            await Recalculate(model.Rows);

            var ids = model.Rows.Select(x => x.Id).ToList();

            var dbRows = await _context.WorkloadRows
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            foreach (var row in model.Rows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == row.Id);

                if (dbRow == null)
                {
                    _context.WorkloadRows.Add(row);
                }
                else
                {
                    dbRow.FlowCount = row.FlowCount;

                    dbRow.StudentsCount = row.StudentsCount;
                    dbRow.GroupCount = row.GroupCount;
                    dbRow.SubgroupCount = row.SubgroupCount;

                    dbRow.LectureTotalHours = row.LectureTotalHours;
                    dbRow.PracticeTotalHours = row.PracticeTotalHours;
                    dbRow.LabTotalHours = row.LabTotalHours;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = model.SelectedYear });
        }

        private async Task Recalculate(List<WorkloadRow> rows)
        {
            var lectureNorm = await GetNorm("Лекции");
            var practiceNorm = await GetNorm("Практические занятия");
            var labNorm = await GetNorm("Лабораторные работы");

            foreach (var row in rows)
            {
                var cont = await _context.ContingentRows
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DirectionCode == row.DirectionCode);

                if (cont == null)
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.SubgroupCount = 0;

                    row.LectureTotalHours = 0;
                    row.PracticeTotalHours = 0;
                    row.LabTotalHours = 0;
                    continue;
                }

                row.StudentsCount = row.Course switch
                {
                    1 => cont.Course1Count,
                    2 => cont.Course2Count,
                    3 => cont.Course3Count,
                    4 => cont.Course4Count,
                    _ => 0
                };

                row.GroupCount = row.Course switch
                {
                    1 => cont.Course1Groups,
                    2 => cont.Course2Groups,
                    3 => cont.Course3Groups,
                    4 => cont.Course4Groups,
                    _ => 0
                };

                row.SubgroupCount = row.Course switch
                {
                    1 => cont.Course1Subgroups,
                    2 => cont.Course2Subgroups,
                    3 => cont.Course3Subgroups,
                    4 => cont.Course4Subgroups,
                    _ => 0
                };

                row.LectureTotalHours = CalculatePlanBasedHours(row.LecturePlanHours, lectureNorm, row);
                row.PracticeTotalHours = CalculatePlanBasedHours(row.PracticePlanHours, practiceNorm, row);
                row.LabTotalHours = CalculatePlanBasedHours(row.LabPlanHours, labNorm, row);
            }
        }

        private decimal CalculatePlanBasedHours(decimal planHours, NormTime? norm, WorkloadRow row)
        {
            if (planHours <= 0 || norm == null)
            {
                return 0;
            }

            var multiplier = GetMultiplier(norm.CalculationBase, row);
            return planHours * multiplier;
        }

        private decimal GetMultiplier(WorkCalculationBase calculationBase, WorkloadRow row)
        {
            return calculationBase switch
            {
                WorkCalculationBase.PerStream => row.FlowCount,
                WorkCalculationBase.PerGroup => row.GroupCount,
                WorkCalculationBase.PerSubgroup => row.SubgroupCount,
                WorkCalculationBase.PerStudent => row.StudentsCount,
                WorkCalculationBase.PerWork => 1,
                WorkCalculationBase.FromLectureHoursTotal => 1,
                _ => 1
            };
        }

        private async Task<NormTime?> GetNorm(string workName)
        {
            return await _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);
        }
        private static string GetSemesterName(int semesterNumber)
        {
            if (semesterNumber <= 0)
                return string.Empty;

            return semesterNumber % 2 == 0 ? "весна" : "осень";
        }
    }
}