using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models.Workload;
using Microsoft.AspNetCore.Mvc;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadCalculationController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            RecalculateRows(AppMemoryStore.WorkloadRows);

            var model = new WorkloadTablePageViewModel
            {
                Rows = AppMemoryStore.WorkloadRows
                    .OrderBy(x => x.Id)
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(WorkloadTablePageViewModel model)
        {
            if (model == null || model.Rows == null || model.Rows.Count == 0)
            {
                TempData["ErrorMessage"] = "Не удалось сохранить таблицу расчета";
                return RedirectToAction(nameof(Index));
            }

            NormalizeRows(model.Rows);
            RecalculateRows(model.Rows);

            AppMemoryStore.WorkloadRows = model.Rows
                .OrderBy(x => x.Id)
                .ToList();

            TempData["SuccessMessage"] = "Таблица расчета нагрузки сохранена";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddRow()
        {
            var newId = AppMemoryStore.WorkloadRows.Count == 0
                ? 1
                : AppMemoryStore.WorkloadRows.Max(x => x.Id) + 1;

            AppMemoryStore.WorkloadRows.Add(new WorkloadTableRowViewModel
            {
                Id = newId,
                SemesterName = "осень",
                EducationForm = "очная",
                Course = 1
            });

            return RedirectToAction(nameof(Index));
        }

        private static void NormalizeRows(List<WorkloadTableRowViewModel> rows)
        {
            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.SemesterName))
                {
                    row.SemesterName = "осень";
                }

                if (string.IsNullOrWhiteSpace(row.EducationForm))
                {
                    row.EducationForm = "очная";
                }

                if (row.Course < 1) row.Course = 1;
                if (row.Course > 4) row.Course = 4;

                if (row.FlowCount < 0) row.FlowCount = 0;
                if (row.GroupCount < 0) row.GroupCount = 0;
                if (row.SubgroupCount < 0) row.SubgroupCount = 0;

                if (row.LecturePlanHours < 0) row.LecturePlanHours = 0;
                if (row.PracticePlanHours < 0) row.PracticePlanHours = 0;
                if (row.LabPlanHours < 0) row.LabPlanHours = 0;
            }
        }

        private static void RecalculateRows(List<WorkloadTableRowViewModel> rows)
        {
            var lectureNorm = GetNormValue("Лекции");
            var practiceNorm = GetNormValue("Практические занятия");
            var labNorm = GetNormValue("Лабораторные занятия");

            foreach (var row in rows)
            {
                row.StudentsCount = GetStudentsCount(row.DirectionCode, row.Course);

                row.LectureTotalHours = row.LecturePlanHours * row.FlowCount * lectureNorm;
                row.PracticeTotalHours = row.PracticePlanHours * row.GroupCount * practiceNorm;
                row.LabTotalHours = row.LabPlanHours * row.SubgroupCount * labNorm;
            }
        }

        private static int GetStudentsCount(string directionCode, int course)
        {
            var contingentRow = AppMemoryStore.Contingent.Rows
                .FirstOrDefault(x => x.DirectionCode == directionCode);

            if (contingentRow == null)
            {
                return 0;
            }

            return course switch
            {
                1 => contingentRow.Course1Count,
                2 => contingentRow.Course2Count,
                3 => contingentRow.Course3Count,
                4 => contingentRow.Course4Count,
                _ => 0
            };
        }

        private static decimal GetNormValue(string workTypeName)
        {
            var norm = AppMemoryStore.NormTimes
                .FirstOrDefault(x => x.IsActive && x.WorkTypeName == workTypeName);

            return norm?.HoursValue ?? 1;
        }
    }
}