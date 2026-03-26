using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.ViewModels.Workload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;

        public WorkloadCalculationController(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rows = await _context.WorkloadRows
                .OrderBy(x => x.Id)
                .ToListAsync();

            await Recalculate(rows);

            return View(new WorkloadTablePageViewModel
            {
                Rows = rows
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(WorkloadTablePageViewModel model)
        {
            await Recalculate(model.Rows);

            var dbRows = await _context.WorkloadRows.ToListAsync();

            foreach (var row in model.Rows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == row.Id);

                if (dbRow == null)
                {
                    _context.WorkloadRows.Add(row);
                }
                else
                {
                    dbRow.DirectionCode = row.DirectionCode;
                    dbRow.DirectionName = row.DirectionName;
                    dbRow.SemesterName = row.SemesterName;
                    dbRow.Course = row.Course;
                    dbRow.EducationForm = row.EducationForm;

                    dbRow.StudentsCount = row.StudentsCount;
                    dbRow.FlowCount = row.FlowCount;
                    dbRow.GroupCount = row.GroupCount;
                    dbRow.SubgroupCount = row.SubgroupCount;

                    dbRow.LecturePlanHours = row.LecturePlanHours;
                    dbRow.PracticePlanHours = row.PracticePlanHours;
                    dbRow.LabPlanHours = row.LabPlanHours;

                    dbRow.LectureTotalHours = row.LectureTotalHours;
                    dbRow.PracticeTotalHours = row.PracticeTotalHours;
                    dbRow.LabTotalHours = row.LabTotalHours;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task Recalculate(List<WorkloadRow> rows)
        {
            var normLecture = await GetNorm("Лекции");
            var normPractice = await GetNorm("Практические занятия");
            var normLab = await GetNorm("Лабораторные работы");

            foreach (var row in rows)
            {
                var cont = await _context.ContingentRows
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

                row.LectureTotalHours = row.LecturePlanHours * row.FlowCount * normLecture;
                row.PracticeTotalHours = row.PracticePlanHours * row.GroupCount * normPractice;
                row.LabTotalHours = row.LabPlanHours * row.SubgroupCount * normLab;
            }
        }

        private async Task<decimal> GetNorm(string workName)
        {
            var norm = await _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);

            return norm?.Hours ?? 1m;
        }
    }
}