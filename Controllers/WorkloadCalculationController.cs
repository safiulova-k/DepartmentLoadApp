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

        public async Task<IActionResult> Index()
        {
            var rows = await _context.WorkloadRows.ToListAsync();

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
                var db = dbRows.FirstOrDefault(x => x.Id == row.Id);

                if (db == null)
                {
                    _context.WorkloadRows.Add(row);
                }
                else
                {
                    db.DirectionCode = row.DirectionCode;
                    db.SemesterName = row.SemesterName;
                    db.Course = row.Course;

                    db.StudentsCount = row.StudentsCount;
                    db.FlowCount = row.FlowCount;
                    db.GroupCount = row.GroupCount;
                    db.SubgroupCount = row.SubgroupCount;

                    db.LecturePlanHours = row.LecturePlanHours;
                    db.PracticePlanHours = row.PracticePlanHours;
                    db.LabPlanHours = row.LabPlanHours;

                    db.LectureTotalHours = row.LectureTotalHours;
                    db.PracticeTotalHours = row.PracticeTotalHours;
                    db.LabTotalHours = row.LabTotalHours;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task Recalculate(List<WorkloadRow> rows)
        {
            foreach (var row in rows)
            {
                var cont = await _context.ContingentRows
                    .FirstOrDefaultAsync(x => x.DirectionCode == row.DirectionCode);

                if (cont == null) continue;

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

                var normLecture = await GetNorm("Лекции");
                var normPractice = await GetNorm("Практические занятия");
                var normLab = await GetNorm("Лабораторные занятия");

                row.LectureTotalHours = row.LecturePlanHours * row.FlowCount * normLecture;
                row.PracticeTotalHours = row.PracticePlanHours * row.GroupCount * normPractice;
                row.LabTotalHours = row.LabPlanHours * row.SubgroupCount * normLab;
            }
        }

        private async Task<decimal> GetNorm(string name)
        {
            var norm = await _context.NormTimes.FirstOrDefaultAsync(x => x.WorkTypeName == name);
            return norm?.HoursValue ?? 1;
        }
    }
}