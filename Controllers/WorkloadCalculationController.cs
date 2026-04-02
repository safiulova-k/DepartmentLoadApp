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

        // Метод для отображения страницы расчета нагрузки
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Загружаем все данные без учета года
            var rows = await _context.WorkloadRows
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            return View(new WorkloadTablePageViewModel
            {
                Rows = rows
            });
        }

        // Метод для импорта данных из учебного плана по учебному году
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromAcademicPlan(string year)
        {
            // Импортируем данные учебного плана за указанный год
            await _academicPlanImportService.ImportYearAsync(year);

            // Загружаем все строки расчета для этого года
            var rows = await _context.WorkloadRows
                .Where(x => x.AcademicYear == year.ToString())
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ToListAsync();

            // Пересчитываем и сохраняем
            await Recalculate(rows);
            await _context.SaveChangesAsync();

            // Перенаправляем на страницу с расчетом для этого года
            return RedirectToAction(nameof(Index), new { year });
        }

        // Метод для сохранения данных расчетов
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(WorkloadTablePageViewModel model)
        {
            var ids = model.Rows.Select(x => x.Id).ToList();

            // Загружаем данные из БД
            var dbRows = await _context.WorkloadRows
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Id)
                .ToListAsync();

            // Обновляем данные
            foreach (var postedRow in model.Rows)
            {
                var dbRow = dbRows.FirstOrDefault(x => x.Id == postedRow.Id);
                if (dbRow == null)
                {
                    continue;
                }

                dbRow.HasExam = postedRow.HasExam;
                dbRow.HasCredit = postedRow.HasCredit;
                dbRow.HasCourseWork = postedRow.HasCourseWork;
                dbRow.HasCourseProject = postedRow.HasCourseProject;
            }

            // Пересчитываем
            await Recalculate(dbRows);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { year = model.SelectedYear });
        }

        // Метод для пересчета нагрузки
        private async Task Recalculate(List<WorkloadRow> rows)
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

            // Проходим по каждой строке расчета
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
                    row.ConsultationHours = 0;
                    row.ExamHours = 0;
                    row.CreditHours = 0;
                    row.CourseWorkHours = 0;
                    row.CourseProjectHours = 0;

                    continue;
                }

                // Заполнение значений студентов и групп
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

                // Рассчитываем часы
                row.LectureTotalHours = CalculatePlanBasedHours(row.LecturePlanHours, lectureNorm, row);
                row.PracticeTotalHours = CalculatePlanBasedHours(row.PracticePlanHours, practiceNorm, row);
                row.LabTotalHours = CalculatePlanBasedHours(row.LabPlanHours, labNorm, row);

                row.ExamHours = CalculateOptionalHours(row.HasExam, examNorm, row);
                row.CreditHours = CalculateOptionalHours(row.HasCredit, creditNorm, row);
                row.CourseWorkHours = CalculateOptionalHours(row.HasCourseWork, courseWorkNorm, row);
                row.CourseProjectHours = CalculateOptionalHours(row.HasCourseProject, courseProjectNorm, row);

                row.ConsultationHours = CalculateConsultationHours(
                    row,
                    consultationNorm,
                    consultationExamExtraNorm);
            }
        }

        // Метод для расчета количества часов на основе плана
        private decimal CalculatePlanBasedHours(decimal planHours, NormTime? norm, WorkloadRow row)
        {
            if (planHours <= 0 || norm == null)
            {
                return 0;
            }

            var multiplier = GetBaseValue(norm.CalculationBase, row);
            var result = planHours * multiplier * norm.Hours;

            return Math.Round(result, 0, MidpointRounding.AwayFromZero);
        }

        // Метод для расчета дополнительных часов
        private decimal CalculateOptionalHours(bool isEnabled, NormTime? norm, WorkloadRow row)
        {
            if (!isEnabled || norm == null)
            {
                return 0;
            }

            var baseValue = GetBaseValue(norm.CalculationBase, row);
            var result = baseValue * norm.Hours;

            return Math.Round(result, 0, MidpointRounding.AwayFromZero);
        }

        // Метод для расчета консультаций
        private decimal CalculateConsultationHours(
            WorkloadRow row,
            NormTime? consultationNorm,
            NormTime? consultationExamExtraNorm)
        {
            decimal result = 0;

            if (consultationNorm != null)
            {
                decimal consultationBase = consultationNorm.CalculationBase switch
                {
                    WorkCalculationBase.FromLectureHoursTotal => row.GroupCount * row.LecturePlanHours,
                    _ => GetBaseValue(consultationNorm.CalculationBase, row)
                };

                result += consultationBase * consultationNorm.Hours;
            }

            if (row.HasExam && consultationExamExtraNorm != null)
            {
                var extraBase = GetBaseValue(consultationExamExtraNorm.CalculationBase, row);
                result += extraBase * consultationExamExtraNorm.Hours;
            }

            return Math.Round(result, 0, MidpointRounding.AwayFromZero);
        }

        // Метод для получения значения на основе типа расчета
        private decimal GetBaseValue(WorkCalculationBase calculationBase, WorkloadRow row)
        {
            return calculationBase switch
            {
                WorkCalculationBase.PerStream => row.FlowCount,
                WorkCalculationBase.PerGroup => row.GroupCount,
                WorkCalculationBase.PerSubgroup => row.SubgroupCount,
                WorkCalculationBase.PerStudent => row.StudentsCount,
                WorkCalculationBase.PerWork => 1,
                WorkCalculationBase.FromLectureHoursTotal => row.GroupCount * row.LecturePlanHours,
                _ => 1
            };
        }

        // Метод для получения нормы по названию работы
        private async Task<NormTime?> GetNormAsync(string workName)
        {
            return await _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);
        }
    }
}