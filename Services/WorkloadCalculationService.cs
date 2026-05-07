using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
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
            var norms = await LoadNormsAsync();

            var contingents = await _context.ContingentRows
                .AsNoTracking()
                .ToListAsync();

            var contingentMap = contingents
                .GroupBy(x => TextNormalizeHelper.Normalize(x.DirectionCode))
                .ToDictionary(x => x.Key, x => x.First());

            var flows = await _context.StudentFlows
                .AsNoTracking()
                .ToListAsync();

            var flowMap = flows
            .GroupBy(x => BuildFlowKey(
                x.AcademicYear,
                TextNormalizeHelper.Normalize(x.DirectionCode),
                x.Course))
            .ToDictionary(x => x.Key, x => x.Count());

            foreach (var row in rows)
            {
                RecalculateSingleRow(row, norms, contingentMap, flowMap);
                row.SourceRowIds = row.Id.ToString();
                row.IsMergedStream = false;
            }
        }

        public async Task<List<WorkloadRow>> BuildRowsForTableAsync(List<WorkloadRow> rows)
        {
            await RecalculateAsync(rows);

            var norms = await LoadNormsAsync();

            return MergeRowsIntoStreams(rows, norms)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ThenBy(x => x.DirectionCode)
                .ToList();
        }

        private void RecalculateSingleRow(
            WorkloadRow row,
            WorkloadNorms norms,
            Dictionary<string, ContingentRow> contingentMap,
            Dictionary<string, int> flowMap)
        {
            var directionCode = TextNormalizeHelper.Normalize(row.DirectionCode);

            if (!contingentMap.TryGetValue(directionCode, out var contingent))
            {
                ResetCalculatedFields(row);
                return;
            }

            row.EducationLevel = ResolveEducationLevel(row, contingent);

            row.StudentsCount = CalculationHelper.GetStudentsByCourse(contingent, row.Course);
            row.GroupCount = CalculationHelper.GetGroupsByCourse(contingent, row.Course);
            row.SubgroupCount = CalculationHelper.GetSubgroupsByCourse(contingent, row.Course);

            var flowKey = BuildFlowKey(row.AcademicYear, directionCode, row.Course);

            row.FlowCount = flowMap.TryGetValue(flowKey, out var flowCount) ? flowCount : 0;

            if (row.FlowCount <= 0)
            {
                row.FlowCount = row.GroupCount > 0 ? 1 : 0;
            }

            CalculateHours(row, norms);
        }

        private List<WorkloadRow> MergeRowsIntoStreams(
            List<WorkloadRow> rows,
            WorkloadNorms norms)
        {
            var result = new List<WorkloadRow>();

            var groups = rows
                .GroupBy(x => new
                {
                    AcademicYear = TextNormalizeHelper.Normalize(x.AcademicYear),
                    EducationLevel = TextNormalizeHelper.Normalize(x.EducationLevel),
                    EducationForm = TextNormalizeHelper.Normalize(x.EducationForm),
                    SemesterName = TextNormalizeHelper.Normalize(x.SemesterName),
                    DisciplineName = TextNormalizeHelper.Normalize(x.DisciplineName),
                    x.Course
                });

            foreach (var group in groups)
            {
                var groupRows = group
                    .OrderBy(x => x.DirectionCode)
                    .ThenBy(x => x.Id)
                    .ToList();

                if (groupRows.Count == 1)
                {
                    var singleRow = groupRows.First();
                    singleRow.SourceRowIds = singleRow.Id.ToString();
                    singleRow.IsMergedStream = false;
                    result.Add(singleRow);
                    continue;
                }

                var first = groupRows.First();

                var mergedRow = new WorkloadRow
                {
                    Id = first.Id,
                    AcademicYear = first.AcademicYear,
                    AcademicPlanId = first.AcademicPlanId,
                    AcademicPlanRecordId = first.AcademicPlanRecordId,
                    DisciplineId = first.DisciplineId,
                    RecordIndex = first.RecordIndex,
                    DisciplineName = first.DisciplineName,
                    DirectionCode = string.Join(", ", groupRows
                        .Select(x => x.DirectionCode)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()),
                    DirectionName = string.Join(", ", groupRows
                        .Select(x => x.DirectionName)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()),
                    SemesterName = first.SemesterName,
                    EducationForm = first.EducationForm,
                    EducationLevel = first.EducationLevel,
                    IsFacultyOptional = groupRows.Any(x => x.IsFacultyOptional),
                    Course = first.Course,

                    StudentsCount = groupRows.Sum(x => x.StudentsCount),
                    GroupCount = groupRows.Sum(x => x.GroupCount),
                    SubgroupCount = groupRows.Sum(x => x.SubgroupCount),

                    FlowCount = groupRows.Sum(x => x.GroupCount) > 0 ? 1 : 0,

                    LecturePlanHours = groupRows.Max(x => x.LecturePlanHours),
                    PracticePlanHours = groupRows.Max(x => x.PracticePlanHours),
                    LabPlanHours = groupRows.Max(x => x.LabPlanHours),

                    HasExam = groupRows.Any(x => x.HasExam),
                    HasCredit = groupRows.Any(x => x.HasCredit),
                    HasCourseWork = groupRows.Any(x => x.HasCourseWork),
                    HasCourseProject = groupRows.Any(x => x.HasCourseProject),
                    HasRgr = groupRows.Any(x => x.HasRgr),

                    SourceRowIds = string.Join(",", groupRows.Select(x => x.Id)),
                    IsMergedStream = true
                };

                CalculateHours(mergedRow, norms);

                result.Add(mergedRow);
            }

            return result;
        }

        private static void CalculateHours(WorkloadRow row, WorkloadNorms norms)
        {
            row.LectureTotalHours = NormCalculationHelper.CalculatePlanHours(
                row.LecturePlanHours,
                norms.LectureNorm,
                row);

            row.PracticeTotalHours = NormCalculationHelper.CalculatePlanHours(
                row.PracticePlanHours,
                norms.PracticeNorm,
                row);

            row.LabTotalHours = NormCalculationHelper.CalculatePlanHours(
                row.LabPlanHours,
                norms.LabNorm,
                row);

            row.ExamHours = NormCalculationHelper.CalculateOptionalHours(
                row.HasExam,
                norms.ExamNorm,
                row);

            row.CreditHours = NormCalculationHelper.CalculateOptionalHours(
                row.HasCredit,
                norms.CreditNorm,
                row);

            row.CourseWorkHours = NormCalculationHelper.CalculateOptionalHours(
                row.HasCourseWork,
                norms.CourseWorkNorm,
                row);

            row.CourseProjectHours = NormCalculationHelper.CalculateOptionalHours(
                row.HasCourseProject,
                norms.CourseProjectNorm,
                row);

            row.RgrHours = NormCalculationHelper.CalculateOptionalHours(
                row.HasRgr,
                norms.RgrNorm,
                row);

            row.ConsultationHours = NormCalculationHelper.CalculateConsultationHours(
                row,
                norms.ConsultationNorm,
                norms.ConsultationExamExtraNorm);
        }

        private async Task<WorkloadNorms> LoadNormsAsync()
        {
            return new WorkloadNorms
            {
                LectureNorm = await GetNormAsync(LectureNormName),
                PracticeNorm = await GetNormAsync(PracticeNormName),
                LabNorm = await GetNormAsync(LabNormName),
                ConsultationNorm = await GetNormAsync(ConsultationNormName),
                ConsultationExamExtraNorm = await GetNormAsync(ConsultationExamExtraNormName),
                ExamNorm = await GetNormAsync(ExamNormName),
                CreditNorm = await GetNormAsync(CreditNormName),
                CourseWorkNorm = await GetNormAsync(CourseWorkNormName),
                CourseProjectNorm = await GetNormAsync(CourseProjectNormName),
                RgrNorm = await GetNormAsync(RgrNormName)
            };
        }

        private async Task<NormTime?> GetNormAsync(string workName)
        {
            return await _context.NormTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkName == workName);
        }

        private static string ResolveEducationLevel(WorkloadRow row, ContingentRow contingent)
        {
            if (!string.IsNullOrWhiteSpace(row.EducationLevel))
            {
                return row.EducationLevel;
            }

            if (contingent.IsBachelor)
            {
                return "Бакалавриат";
            }

            if (contingent.IsMaster)
            {
                return "Магистратура";
            }

            if (row.DirectionCode.Contains(".04."))
            {
                return "Магистратура";
            }

            return "Бакалавриат";
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
            row.SourceRowIds = row.Id.ToString();
            row.IsMergedStream = false;
        }
        private static string BuildFlowKey(string academicYear, string directionCode, int course)
        {
            return $"{TextNormalizeHelper.Normalize(academicYear)}|{TextNormalizeHelper.Normalize(directionCode)}|{course}";
        }
        private sealed class WorkloadNorms
        {
            public NormTime? LectureNorm { get; set; }

            public NormTime? PracticeNorm { get; set; }

            public NormTime? LabNorm { get; set; }

            public NormTime? ConsultationNorm { get; set; }

            public NormTime? ConsultationExamExtraNorm { get; set; }

            public NormTime? ExamNorm { get; set; }

            public NormTime? CreditNorm { get; set; }

            public NormTime? CourseWorkNorm { get; set; }

            public NormTime? CourseProjectNorm { get; set; }

            public NormTime? RgrNorm { get; set; }
        }
    }
}