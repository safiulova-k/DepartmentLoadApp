using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Workload;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services
{
    public class WorkloadCalculationService
    {
        private const string LectureNormName = "Лекции";
        private const string PracticeNormName = "Практические занятия";
        private const string LabNormName = "Лабораторные работы";
        private const string ConsultationNormName = "Консультации";
        private const string ConsultationExamExtraNormName = "Доп. консультация к экзамену";
        private const string ExamNormName = "Экзамен";
        private const string CreditNormName = "Зачет";
        private const string CourseWorkNormName = "Курсовая работа";
        private const string CourseProjectNormName = "Курсовой проект";
        private const string RgrNormName = "РГР";

        private readonly DepartmentLoadDbContext _context;

        public WorkloadCalculationService(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        public async Task RecalculateAsync(List<WorkloadRow> rows)
        {
            var lectureNorm = await GetNormAsync(LectureNormName);
            var practiceNorm = await GetNormAsync(PracticeNormName);
            var labNorm = await GetNormAsync(LabNormName);
            var consultationNorm = await GetNormAsync(ConsultationNormName);
            var consultationExamExtraNorm = await GetNormAsync(ConsultationExamExtraNormName);
            var examNorm = await GetNormAsync(ExamNormName);
            var creditNorm = await GetNormAsync(CreditNormName);
            var courseWorkNorm = await GetNormAsync(CourseWorkNormName);
            var courseProjectNorm = await GetNormAsync(CourseProjectNormName);
            var rgrNorm = await GetNormAsync(RgrNormName);

            var contingents = await _context.ContingentRows
                .AsNoTracking()
                .ToListAsync();

            var contingentMap = contingents
                .GroupBy(x => NormalizeText(x.DirectionCode))
                .ToDictionary(x => x.Key, x => x.First());

            var flows = await _context.StudentFlows
                .AsNoTracking()
                .ToListAsync();

            var flowMap = flows
                .GroupBy(x => new
                {
                    x.AcademicYear,
                    DirectionCode = NormalizeText(x.DirectionCode),
                    x.Course
                })
                .ToDictionary(x => x.Key, x => x.Count());

            foreach (var row in rows)
            {
                var directionCode = NormalizeText(row.DirectionCode);

                if (!contingentMap.TryGetValue(directionCode, out var contingent))
                {
                    ResetCalculatedFields(row);
                    continue;
                }

                row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
                row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);
                row.SubgroupCount = CalculationHelper.GetSubgroupsByCourse(contingent, row.Course);

                var flowKey = new
                {
                    row.AcademicYear,
                    DirectionCode = directionCode,
                    row.Course
                };

                row.FlowCount = flowMap.TryGetValue(flowKey, out var flowCount)
                    ? flowCount
                    : 0;

                if (row.FlowCount <= 0)
                {
                    row.FlowCount = row.GroupCount > 0 ? 1 : 0;
                }

                row.LectureTotalHours = NormCalculationHelper.CalculatePlanHours(
                    row.LecturePlanHours,
                    lectureNorm,
                    row);

                row.PracticeTotalHours = NormCalculationHelper.CalculatePlanHours(
                    row.PracticePlanHours,
                    practiceNorm,
                    row);

                row.LabTotalHours = NormCalculationHelper.CalculatePlanHours(
                    row.LabPlanHours,
                    labNorm,
                    row);

                row.ExamHours = NormCalculationHelper.CalculateOptionalHours(
                    row.HasExam,
                    examNorm,
                    row);

                row.CreditHours = NormCalculationHelper.CalculateOptionalHours(
                    row.HasCredit,
                    creditNorm,
                    row);

                row.CourseWorkHours = NormCalculationHelper.CalculateOptionalHours(
                    row.HasCourseWork,
                    courseWorkNorm,
                    row);

                row.CourseProjectHours = NormCalculationHelper.CalculateOptionalHours(
                    row.HasCourseProject,
                    courseProjectNorm,
                    row);

                row.RgrHours = NormCalculationHelper.CalculateOptionalHours(
                    row.HasRgr,
                    rgrNorm,
                    row);

                row.ConsultationHours = NormCalculationHelper.CalculateConsultationHours(
                    row,
                    consultationNorm,
                    consultationExamExtraNorm);
            }
        }

        private async Task<NormTime?> GetNormAsync(string workName)
        {
            return await _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);
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
    }
}