using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.GiaMock;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.ViewModels.Gia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class GiaCalculationController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IGiaWorkloadImportService _giaWorkloadImportService;

        public GiaCalculationController(
            DepartmentLoadDbContext context,
            IGiaWorkloadImportService giaWorkloadImportService)
        {
            _context = context;
            _giaWorkloadImportService = giaWorkloadImportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;

            await _giaWorkloadImportService.EnsureYearImportedAsync(selectedYear);

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
                Rows = rows
            });
        }

        [HttpPost]
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
                {
                    continue;
                }

                // Пока вручную редактируются только консультации к госэкзамену
                if (dbRow.WorkName == "Консультация к госэкзамену")
                {
                    dbRow.ManualHours = row.ManualHours;
                }
            }

            await RecalculateAsync(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = model.SelectedYear });
        }

        private async Task RecalculateAsync(List<GiaWorkloadRow> rows)
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .Where(x => x.CategoryName == "ГИА")
                .ToListAsync();

            foreach (var row in rows)
            {
                var contingent = await _context.ContingentRows
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DirectionCode == row.DirectionCode);

                if (contingent == null)
                {
                    row.StudentsCount = 0;
                    row.GroupCount = 0;
                    row.TotalHours = 0;
                    continue;
                }

                row.StudentsCount = GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = GetGroupsByCourse(contingent, row.Course);

                NormalizeGiaWorkNameByQualification(row, contingent);

                row.TotalHours = CalculateGiaHours(row, norms);
            }
        }

        private static void NormalizeGiaWorkNameByQualification(GiaWorkloadRow row, ContingentRow contingent)
        {
            if (row.WorkName != "Руководство ВКР")
            {
                return;
            }

            if (contingent.IsMaster)
            {
                row.WorkName = "Руководство ВКР магистра";
            }
            else
            {
                row.WorkName = "Руководство ВКР бакалавра";
            }
        }

        private decimal CalculateGiaHours(GiaWorkloadRow row, List<NormTime> norms)
        {
            // Спецслучай: консультация к госэкзамену пока задаётся вручную / из JSON
            if (row.WorkName == "Консультация к госэкзамену")
            {
                return RoundHours(row.ManualHours);
            }

            var norm = norms.FirstOrDefault(x => x.WorkName == row.WorkName);
            if (norm == null)
            {
                return 0;
            }

            var result = CalculateByNorm(
                calculationBase: norm.CalculationBase,
                coefficient: norm.Hours,
                studentsCount: row.StudentsCount,
                groupCount: row.GroupCount,
                subgroupCount: 0,
                streamCount: 0,
                planHours: 0);

            return RoundHours(result);
        }

        private static decimal CalculateByNorm(
            WorkCalculationBase calculationBase,
            decimal coefficient,
            int studentsCount,
            int groupCount,
            int subgroupCount,
            int streamCount,
            decimal planHours)
        {
            return calculationBase switch
            {
                WorkCalculationBase.PerStudent => studentsCount * coefficient,
                WorkCalculationBase.PerGroup => groupCount * coefficient,
                WorkCalculationBase.PerSubgroup => subgroupCount * coefficient,
                WorkCalculationBase.PerStream => streamCount * coefficient,

                // Если у тебя в enum уже есть такая основа, оставь.
                // Если нет — просто удали этот case.
                WorkCalculationBase.FromLectureHoursTotal => planHours * coefficient,

                _ => 0
            };
        }

        private static decimal RoundHours(decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        private static int GetStudentsByCourse(ContingentRow contingent, int course)
        {
            return course switch
            {
                1 => contingent.Course1Count,
                2 => contingent.Course2Count,
                3 => contingent.Course3Count,
                4 => contingent.Course4Count,
                _ => 0
            };
        }

        private static int GetGroupsByCourse(ContingentRow contingent, int course)
        {
            return course switch
            {
                1 => contingent.Course1Groups,
                2 => contingent.Course2Groups,
                3 => contingent.Course3Groups,
                4 => contingent.Course4Groups,
                _ => 0
            };
        }
    }
}