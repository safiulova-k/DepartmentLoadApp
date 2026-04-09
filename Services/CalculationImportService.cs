using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models.Core;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.Models.Workload;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services
{
    public class CalculationImportService
    {
        private readonly DepartmentLoadDbContext _context;

        public CalculationImportService(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        public async Task ImportAllAsync(int? startYear)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            var plans = await _context.AcademicPlansCore
                .AsNoTracking()
                .ToListAsync();

            var planIds = plans.Select(x => x.Id).ToList();

            var records = await _context.AcademicPlanRecordsCore
                .AsNoTracking()
                .Where(x => planIds.Contains(x.AcademicPlanId))
                .ToListAsync();

            var directions = await _context.EducationDirections
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id);

            var existingWorkloadRows = await _context.WorkloadRows
                .Where(x => x.AcademicYear == selectedYear)
                .ToListAsync();

            var existingPracticeRows = await _context.PracticeWorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .ToListAsync();

            var existingGiaRows = await _context.GiaWorkloadRows
                .Where(x => x.PlanYear == selectedYear)
                .ToListAsync();

            var weeksByRecordId = existingPracticeRows
                .Where(x => x.AcademicPlanRecordId > 0)
                .GroupBy(x => x.AcademicPlanRecordId)
                .ToDictionary(x => x.Key, x => x.First().WeeksCount);

            var manualHoursMap = existingGiaRows
                .Where(x => x.AcademicPlanRecordId > 0)
                .ToDictionary(
                    x => BuildGiaManualKey(x.AcademicPlanRecordId, x.WorkName),
                    x => x.ManualHours);

            if (existingWorkloadRows.Any())
                _context.WorkloadRows.RemoveRange(existingWorkloadRows);

            if (existingPracticeRows.Any())
                _context.PracticeWorkloadRows.RemoveRange(existingPracticeRows);

            if (existingGiaRows.Any())
                _context.GiaWorkloadRows.RemoveRange(existingGiaRows);

            var workloadRows = BuildDisciplineRows(
                selectedYearStart, selectedYear, plans, records, directions);

            var practiceRows = BuildPracticeRows(
                selectedYearStart, selectedYear, plans, records, directions, weeksByRecordId);

            var giaRows = BuildGiaRows(
                selectedYearStart, selectedYear, plans, records, directions, manualHoursMap);

            if (workloadRows.Any())
                await _context.WorkloadRows.AddRangeAsync(workloadRows);

            if (practiceRows.Any())
                await _context.PracticeWorkloadRows.AddRangeAsync(practiceRows);

            if (giaRows.Any())
                await _context.GiaWorkloadRows.AddRangeAsync(giaRows);

            await _context.SaveChangesAsync();
        }

        private static List<WorkloadRow> BuildDisciplineRows(
            int selectedYearStart,
            string selectedYear,
            List<AcademicPlan> plans,
            List<AcademicPlanRecord> records,
            Dictionary<int, EducationDirection> directions)
        {
            var result = new List<WorkloadRow>();

            foreach (var plan in plans)
            {
                if (plan.EducationDirectionId == null)
                    continue;

                if (!directions.TryGetValue(plan.EducationDirectionId.Value, out var direction))
                    continue;

                if (!AcademicYearResolver.TryResolveCourseAndSemesters(
                        plan.Year,
                        selectedYearStart,
                        out var course,
                        out var semesters))
                    continue;

                var planRecords = records
                    .Where(x => x.AcademicPlanId == plan.Id)
                    .Where(x => semesters.Contains(x.Semester))
                    .Where(x => IsDisciplineRecord(x.Index))
                    .ToList();

                foreach (var record in planRecords)
                {
                    result.Add(new WorkloadRow
                    {
                        AcademicYear = selectedYear,
                        AcademicPlanId = plan.Id,
                        AcademicPlanRecordId = record.Id,
                        DirectionCode = direction.Cipher,
                        DirectionName = direction.Title,
                        DisciplineName = record.Name ?? string.Empty,
                        SemesterName = AcademicYearResolver.GetSemesterName(record.Semester),
                        EducationForm = GetEducationFormName(plan),
                        Course = course,
                        LecturePlanHours = record.Lectures ?? 0,
                        PracticePlanHours = record.PracticalHours ?? 0,
                        LabPlanHours = record.LaboratoryHours ?? 0,
                        HasExam = (record.Exam ?? 0) > 0,
                        HasCredit = (record.Pass ?? 0) > 0 || (record.GradedPass ?? 0) > 0,
                        HasCourseWork = (record.CourseWork ?? 0) > 0,
                        HasCourseProject = (record.CourseProject ?? 0) > 0,
                        HasRgr = (record.Rgr ?? 0) > 0
                    });
                }
            }

            return result;
        }

        private static List<PracticeWorkloadRow> BuildPracticeRows(
            int selectedYearStart,
            string selectedYear,
            List<AcademicPlan> plans,
            List<AcademicPlanRecord> records,
            Dictionary<int, EducationDirection> directions,
            Dictionary<int, int> weeksByRecordId)
        {
            var result = new List<PracticeWorkloadRow>();

            foreach (var plan in plans)
            {
                if (plan.EducationDirectionId == null)
                    continue;

                if (!directions.TryGetValue(plan.EducationDirectionId.Value, out var direction))
                    continue;

                if (!AcademicYearResolver.TryResolveCourseAndSemesters(
                        plan.Year,
                        selectedYearStart,
                        out var course,
                        out var semesters))
                    continue;

                var planRecords = records
                    .Where(x => x.AcademicPlanId == plan.Id)
                    .Where(x => semesters.Contains(x.Semester))
                    .Where(x => IsPracticeRecord(x.Index))
                    .ToList();

                foreach (var record in planRecords)
                {
                    result.Add(new PracticeWorkloadRow
                    {
                        PlanYear = selectedYear,
                        AcademicPlanId = plan.Id,
                        AcademicPlanRecordId = record.Id,
                        PracticeName = NormalizePracticeName(record.Name),
                        DirectionCode = direction.Cipher,
                        DirectionName = direction.Title,
                        Course = course,
                        SemesterName = AcademicYearResolver.GetSemesterName(record.Semester),
                        EducationForm = GetEducationFormName(plan),
                        WeeksCount = weeksByRecordId.TryGetValue(record.Id, out var weeks) ? weeks : 0
                    });
                }
            }

            return result;
        }

        private static List<GiaWorkloadRow> BuildGiaRows(
            int selectedYearStart,
            string selectedYear,
            List<AcademicPlan> plans,
            List<AcademicPlanRecord> records,
            Dictionary<int, EducationDirection> directions,
            Dictionary<string, decimal> manualHoursMap)
        {
            var result = new List<GiaWorkloadRow>();

            foreach (var plan in plans)
            {
                if (plan.EducationDirectionId == null)
                    continue;

                if (!directions.TryGetValue(plan.EducationDirectionId.Value, out var direction))
                    continue;

                if (!AcademicYearResolver.TryResolveCourseAndSemesters(
                        plan.Year,
                        selectedYearStart,
                        out var course,
                        out var semesters))
                    continue;

                var planRecords = records
                    .Where(x => x.AcademicPlanId == plan.Id)
                    .Where(x => semesters.Contains(x.Semester))
                    .Where(x => IsGiaRecord(x.Index))
                    .ToList();

                foreach (var record in planRecords)
                {
                    var semesterName = AcademicYearResolver.GetSemesterName(record.Semester);
                    var educationForm = GetEducationFormName(plan);

                    if (IsStateExamRecord(record))
                    {
                        AddGiaRow(
                            result, selectedYear, plan.Id, record.Id,
                            "Госэкзамен", "Консультация к госэкзамену",
                            direction.Cipher, direction.Title, course, semesterName, educationForm, manualHoursMap);

                        AddGiaRow(
                            result, selectedYear, plan.Id, record.Id,
                            "Госэкзамен", "Госэкзамен",
                            direction.Cipher, direction.Title, course, semesterName, educationForm, manualHoursMap);

                        continue;
                    }

                    AddGiaRow(
                        result, selectedYear, plan.Id, record.Id,
                        "Дипломное проектирование", "Руководство ВКР",
                        direction.Cipher, direction.Title, course, semesterName, educationForm, manualHoursMap);

                    AddGiaRow(
                        result, selectedYear, plan.Id, record.Id,
                        "Дипломное проектирование", "Нормоконтроль ВКР",
                        direction.Cipher, direction.Title, course, semesterName, educationForm, manualHoursMap);

                    AddGiaRow(
                        result, selectedYear, plan.Id, record.Id,
                        "ГЭК", "Работа в ГЭК",
                        direction.Cipher, direction.Title, course, semesterName, educationForm, manualHoursMap);
                }
            }

            return result;
        }

        private static bool IsDisciplineRecord(string? index)
        {
            if (string.IsNullOrWhiteSpace(index))
                return false;

            var normalized = index.Trim().ToUpperInvariant();
            return normalized.StartsWith("Б1.") || normalized.StartsWith("ФТД");
        }

        private static bool IsPracticeRecord(string? index)
        {
            if (string.IsNullOrWhiteSpace(index))
                return false;

            var normalized = index.Trim().ToUpperInvariant();
            return normalized.StartsWith("Б2");
        }

        private static bool IsGiaRecord(string? index)
        {
            if (string.IsNullOrWhiteSpace(index))
                return false;

            var normalized = index.Trim().ToUpperInvariant();
            return normalized.StartsWith("Б3");
        }

        private static bool IsStateExamRecord(AcademicPlanRecord record)
        {
            if ((record.Exam ?? 0) > 0)
                return true;

            var name = (record.Name ?? string.Empty).ToLowerInvariant();
            return name.Contains("государствен") && name.Contains("экзам");
        }

        private static void AddGiaRow(
            List<GiaWorkloadRow> rows,
            string planYear,
            int academicPlanId,
            int academicPlanRecordId,
            string giaSection,
            string workName,
            string directionCode,
            string directionName,
            int course,
            string semesterName,
            string educationForm,
            Dictionary<string, decimal> manualHoursMap)
        {
            var key = BuildGiaManualKey(academicPlanRecordId, workName);

            rows.Add(new GiaWorkloadRow
            {
                PlanYear = planYear,
                AcademicPlanId = academicPlanId,
                AcademicPlanRecordId = academicPlanRecordId,
                GiaSection = giaSection,
                WorkName = workName,
                DirectionCode = directionCode,
                DirectionName = directionName,
                Course = course,
                SemesterName = semesterName,
                EducationForm = educationForm,
                ManualHours = manualHoursMap.TryGetValue(key, out var manualHours) ? manualHours : 0
            });
        }

        private static string BuildGiaManualKey(int recordId, string workName)
            => $"{recordId}|{workName}";

        private static string NormalizePracticeName(string? sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                return string.Empty;

            var value = sourceName.Trim().ToLowerInvariant();

            if (value.Contains("ознаком"))
                return "Ознакомительная практика";

            if (value.Contains("технолог") || value.Contains("производствен"))
                return "Технологическая практика";

            if (value.Contains("преддиплом") && value.Contains("магистр"))
                return "Преддипломная практика магистров";

            if (value.Contains("преддиплом"))
                return "Преддипломная практика бакалавров";

            if (value.Contains("нирм"))
                return "НИРМ";

            if (value.Contains("научно-исследовательская"))
                return "Научно-исследовательская работа";

            if (value == "нир" || value.Contains(" нир"))
                return "НИР";

            if (value.Contains("учебн"))
                return "Учебная практика";

            return sourceName.Trim();
        }

        private static string GetEducationFormName(AcademicPlan plan)
            => plan.EducationForm.ToString();
    }
}