using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Core;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.ViewModels.WorkloadDistribution;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services
{
    public class WorkloadDistributionService
    {
        private const decimal MinLecturerRate = 0m;
        private const decimal MaxLecturerRate = 2.00m;
        private const string AssistantPostKeyword = "ассистент";

        private const string LectureNormName = "Лекции";
        private const string PracticeNormName = "Практические занятия";
        private const string LabNormName = "Лабораторные работы";
        private const string ConsultationNormName = "Консультации";
        private const string ExamNormName = "Экзамен";
        private const string CreditNormName = "Зачет";
        private const string CourseWorkNormName = "Курсовая работа";
        private const string CourseProjectNormName = "Курсовой проект";
        private const string RgrNormName = "РГР";

        private readonly DepartmentLoadDbContext _context;
        private readonly WorkloadCalculationService _workloadCalculationService;
        private readonly PracticeCalculationService _practiceCalculationService;
        private readonly GiaCalculationService _giaCalculationService;

        public WorkloadDistributionService(
            DepartmentLoadDbContext context,
            WorkloadCalculationService workloadCalculationService,
            PracticeCalculationService practiceCalculationService,
            GiaCalculationService giaCalculationService)
        {
            _context = context;
            _workloadCalculationService = workloadCalculationService;
            _practiceCalculationService = practiceCalculationService;
            _giaCalculationService = giaCalculationService;
        }

        public async Task<WorkloadDistributionPageViewModel> BuildPageAsync(
            int? startYear,
            int? selectedLecturerId = null)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var academicYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            await EnsureAcademicYearPlansAsync(academicYear);

            var studyPosts = await _context.LecturerStudyPosts
                .AsNoTracking()
                .OrderBy(x => x.StudyPostTitle)
                .ToListAsync();

            var plans = await _context.LecturerAcademicYearPlans
                .Include(x => x.Lecturer)
                .Include(x => x.LecturerStudyPost)
                .Where(x => x.AcademicYear == academicYear)
                .OrderBy(x => x.Lecturer!.LastName)
                .ThenBy(x => x.Lecturer!.FirstName)
                .ThenBy(x => x.Lecturer!.Patronymic)
                .ToListAsync();

            var assignments = await _context.LecturerLoadAssignments
                .Where(x => x.AcademicYear == academicYear)
                .OrderBy(x => x.Id)
                .ToListAsync();

            var items = await BuildDistributableItemsAsync(academicYear, assignments);

            var itemMap = items
                .GroupBy(BuildItemKey)
                .ToDictionary(x => x.Key, x => x.First());

            var validAssignments = assignments
                .Where(x => itemMap.ContainsKey(BuildAssignmentKey(x)))
                .ToList();

            var selectedId = selectedLecturerId;

            if (!selectedId.HasValue && plans.Count > 0)
            {
                selectedId = plans[0].LecturerId;
            }

            var page = new WorkloadDistributionPageViewModel
            {
                SelectedYearStart = selectedYearStart,
                SelectedYear = academicYear,
                AvailableYearStarts = AcademicYearResolver.BuildAvailableStartYears(selectedYearStart),
                SelectedLecturerId = selectedId,
                TotalHours = items.Sum(x => x.TotalHours),
                AssignedHours = validAssignments.Sum(x => x.AssignedHours),
                RemainingHours = Math.Max(0, items.Sum(x => x.TotalHours) - validAssignments.Sum(x => x.AssignedHours)),
                StudyPosts = studyPosts
                    .Select(x => new WorkloadDistributionStudyPostOptionViewModel
                    {
                        Id = x.Id,
                        Title = x.StudyPostTitle,
                        NormHours = x.Hours
                    })
                    .ToList(),
                RemainingItems = items
                    .Where(x => x.RemainingHours > 0)
                    .OrderBy(x => x.SemesterName)
                    .ThenBy(x => x.Title)
                    .ThenBy(x => x.ElementDisplayName)
                    .ThenBy(x => x.UnitName)
                    .Select(MapAvailableItem)
                    .ToList()
            };

            foreach (var plan in plans)
            {
                var lecturerAssignments = validAssignments
                    .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
                    .ToList();

                var assignedHours = lecturerAssignments.Sum(x => x.AssignedHours);
                var limitHours = CalculateLimitHours(plan.LecturerStudyPost?.Hours ?? 0, plan.Rate);
                var isAssistant = IsAssistant(plan.LecturerStudyPost?.StudyPostTitle);

                var card = new WorkloadDistributionLecturerCardViewModel
                {
                    LecturerId = plan.LecturerId,
                    LecturerDisplayName = GetLecturerDisplayName(plan.Lecturer),
                    LecturerStudyPostId = plan.LecturerStudyPostId,
                    LecturerStudyPostTitle = plan.LecturerStudyPost?.StudyPostTitle ?? "Не выбрана",
                    Rate = plan.Rate,
                    NormHours = plan.LecturerStudyPost?.Hours ?? 0,
                    LimitHours = limitHours,
                    AssignedHours = assignedHours,
                    RemainingHours = limitHours - assignedHours,
                    IsAssistant = isAssistant,
                    IsOverloaded = assignedHours > limitHours
                };

                var availableForLecturer = items
                    .Where(x => x.RemainingHours > 0)
                    .Where(x => !(isAssistant && x.LoadElementType == LoadAssignmentElementType.Lecture))
                    .OrderBy(x => x.SemesterName)
                    .ThenBy(x => x.Title)
                    .ThenBy(x => x.ElementDisplayName)
                    .ThenBy(x => x.UnitName)
                    .ToList();

                card.AvailableItems = availableForLecturer
                    .Select(MapAvailableItem)
                    .ToList();

                card.SemesterGroups = BuildSemesterGroups(availableForLecturer);
                card.GiaItems = BuildGiaItems(availableForLecturer);

                foreach (var assignment in lecturerAssignments)
                {
                    if (!itemMap.TryGetValue(BuildAssignmentKey(assignment), out var item))
                    {
                        continue;
                    }

                    card.Assignments.Add(new WorkloadDistributionAssignmentViewModel
                    {
                        AssignmentId = assignment.Id,
                        SourceTypeDisplayName = GetSourceDisplayName(assignment.SourceType),
                        Title = item.Title,
                        Subtitle = item.Subtitle,
                        ElementDisplayName = item.ElementDisplayName,
                        UnitName = assignment.UnitName,
                        StudentsCount = assignment.StudentsCount,
                        AssignedHours = assignment.AssignedHours,
                        TotalItemHours = item.TotalHours,
                        RemainingItemHours = item.RemainingHours
                    });
                }

                card.Assignments = card.Assignments
                    .OrderBy(x => x.SourceTypeDisplayName)
                    .ThenBy(x => x.Title)
                    .ThenBy(x => x.ElementDisplayName)
                    .ThenBy(x => x.UnitName)
                    .ToList();

                page.Lecturers.Add(card);
            }

            page.OverloadedLecturerCount = page.Lecturers.Count(x => x.IsOverloaded);

            return page;
        }

        public async Task<WorkloadDistributionOperationResult> SaveLecturerPlanAsync(
            int selectedYearStart,
            int lecturerId,
            int? lecturerStudyPostId,
            decimal rate)
        {
            var academicYear = AcademicYearResolver.BuildAcademicYear(
                AcademicYearResolver.NormalizeStartYear(selectedYearStart));

            await EnsureAcademicYearPlansAsync(academicYear);

            var normalizedRate = Math.Round(rate, 2, MidpointRounding.AwayFromZero);

            if (normalizedRate < MinLecturerRate || normalizedRate > MaxLecturerRate)
            {
                return WorkloadDistributionOperationResult.Fail(
                    $"Ставка должна быть в диапазоне от {MinLecturerRate} до {MaxLecturerRate}.",
                    lecturerId);
            }

            var plan = await _context.LecturerAcademicYearPlans
                .Include(x => x.LecturerStudyPost)
                .FirstOrDefaultAsync(x =>
                    x.AcademicYear == academicYear &&
                    x.LecturerId == lecturerId);

            if (plan == null)
            {
                return WorkloadDistributionOperationResult.Fail(
                    "План преподавателя не найден.",
                    lecturerId);
            }
            if (!lecturerStudyPostId.HasValue)
            {
                return WorkloadDistributionOperationResult.Fail(
                    "Выберите должность преподавателя.",
                    lecturerId);
            }
            LecturerStudyPost? newStudyPost = null;

            if (lecturerStudyPostId.HasValue)
            {
                newStudyPost = await _context.LecturerStudyPosts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == lecturerStudyPostId.Value);

                if (newStudyPost == null)
                {
                    return WorkloadDistributionOperationResult.Fail(
                        "Учебная должность не найдена.",
                        lecturerId);
                }
            }

            var currentAssignedHours = await _context.LecturerLoadAssignments
                .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
                .SumAsync(x => (decimal?)x.AssignedHours) ?? 0m;

            var newLimitHours = CalculateLimitHours(newStudyPost?.Hours ?? 0, normalizedRate);

            if (currentAssignedHours > newLimitHours)
            {
                return WorkloadDistributionOperationResult.Fail(
                    $"Нельзя сохранить: у преподавателя уже назначено {currentAssignedHours:0.##} ч., а по новой ставке и должности можно только {newLimitHours:0.##} ч.",
                    lecturerId);
            }

            if (IsAssistant(newStudyPost?.StudyPostTitle))
            {
                var hasLectureAssignments = await _context.LecturerLoadAssignments
                    .AnyAsync(x =>
                        x.LecturerAcademicYearPlanId == plan.Id &&
                        x.LoadElementType == LoadAssignmentElementType.Lecture);

                if (hasLectureAssignments)
                {
                    return WorkloadDistributionOperationResult.Fail(
                        "Нельзя назначить должность ассистента, пока у преподавателя есть лекции.",
                        lecturerId);
                }
            }

            plan.LecturerStudyPostId = lecturerStudyPostId;
            plan.Rate = normalizedRate;

            await _context.SaveChangesAsync();

            return WorkloadDistributionOperationResult.Ok(
                "Параметры преподавателя сохранены.",
                lecturerId);
        }

        public async Task<WorkloadDistributionOperationResult> AddSelectedAssignmentsAsync(
            int selectedYearStart,
            int lecturerId,
            List<string> selectedItemKeys,
            List<GiaStudentsAssignmentInputModel> giaStudents)
        {
            var academicYear = AcademicYearResolver.BuildAcademicYear(
                AcademicYearResolver.NormalizeStartYear(selectedYearStart));

            await EnsureAcademicYearPlansAsync(academicYear);

            var plan = await _context.LecturerAcademicYearPlans
                .Include(x => x.LecturerStudyPost)
                .FirstOrDefaultAsync(x =>
                    x.AcademicYear == academicYear &&
                    x.LecturerId == lecturerId);

            if (plan == null)
            {
                return WorkloadDistributionOperationResult.Fail(
                    "План преподавателя не найден.",
                    lecturerId);
            }

            selectedItemKeys ??= new List<string>();
            giaStudents ??= new List<GiaStudentsAssignmentInputModel>();

            var yearAssignments = await _context.LecturerLoadAssignments
                .AsNoTracking()
                .Where(x => x.AcademicYear == academicYear)
                .ToListAsync();

            var availableItems = await BuildDistributableItemsAsync(academicYear, yearAssignments);

            var selectedItems = new List<DistributableLoadItem>();

            foreach (var itemKey in selectedItemKeys.Distinct())
            {
                if (!TryParseKey(itemKey, out var parsed))
                {
                    return WorkloadDistributionOperationResult.Fail(
                        "Некорректный элемент нагрузки.",
                        lecturerId);
                }

                var item = availableItems.FirstOrDefault(x => BuildItemKey(x) == itemKey);

                if (item == null || item.RemainingHours <= 0)
                {
                    return WorkloadDistributionOperationResult.Fail(
                        "Один из выбранных элементов уже распределён.",
                        lecturerId);
                }

                if (IsAssistant(plan.LecturerStudyPost?.StudyPostTitle)
                    && item.LoadElementType == LoadAssignmentElementType.Lecture)
                {
                    return WorkloadDistributionOperationResult.Fail(
                        "Ассистенту нельзя назначать лекции.",
                        lecturerId);
                }

                if (item.SourceType == LoadAssignmentSourceType.Gia)
                {
                    continue;
                }

                selectedItems.Add(item);
            }

            var giaSelectedItems = new List<(DistributableLoadItem Item, int StudentsCount, decimal Hours)>();

            foreach (var input in giaStudents.Where(x => x.StudentsCount > 0))
            {
                var item = availableItems.FirstOrDefault(x => BuildItemKey(x) == input.ItemKey);

                if (item == null || item.SourceType != LoadAssignmentSourceType.Gia)
                {
                    return WorkloadDistributionOperationResult.Fail(
                        "Некорректная строка ГИА.",
                        lecturerId);
                }

                if (input.StudentsCount > item.RemainingStudentsCount)
                {
                    return WorkloadDistributionOperationResult.Fail(
                        $"По строке «{item.ElementDisplayName}» осталось только {item.RemainingStudentsCount} студентов.",
                        lecturerId);
                }

                var hours = RoundHours(input.StudentsCount * item.HoursPerStudent);

                if (hours <= 0)
                {
                    continue;
                }

                giaSelectedItems.Add((item, input.StudentsCount, hours));
            }

            if (!selectedItems.Any() && !giaSelectedItems.Any())
            {
                return WorkloadDistributionOperationResult.Fail(
                    "Выберите хотя бы один элемент нагрузки.",
                    lecturerId);
            }

            var lecturerAssignedHours = yearAssignments
                .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
                .Sum(x => x.AssignedHours);

            var limitHours = CalculateLimitHours(plan.LecturerStudyPost?.Hours ?? 0, plan.Rate);
            var selectedHours = selectedItems.Sum(x => x.RemainingHours) + giaSelectedItems.Sum(x => x.Hours);

            if (lecturerAssignedHours + selectedHours > limitHours)
            {
                return WorkloadDistributionOperationResult.Fail(
                    $"Нельзя назначить нагрузку: выбрано {selectedHours:0.##} ч., свободно у преподавателя {Math.Max(0, limitHours - lecturerAssignedHours):0.##} ч.",
                    lecturerId);
            }

            foreach (var item in selectedItems)
            {
                _context.LecturerLoadAssignments.Add(new LecturerLoadAssignment
                {
                    AcademicYear = academicYear,
                    LecturerAcademicYearPlanId = plan.Id,
                    SourceType = item.SourceType,
                    SourceRowId = item.SourceRowId,
                    SourceAcademicPlanRecordId = item.SourceAcademicPlanRecordId,
                    LoadElementType = item.LoadElementType,
                    DistributionUnitType = item.DistributionUnitType,
                    StudentGroupId = item.StudentGroupId,
                    ContingentSubgroupId = item.ContingentSubgroupId,
                    UnitName = item.UnitName,
                    StudentsCount = item.StudentsCount,
                    AssignedHours = item.RemainingHours
                });
            }

            foreach (var giaItem in giaSelectedItems)
            {
                _context.LecturerLoadAssignments.Add(new LecturerLoadAssignment
                {
                    AcademicYear = academicYear,
                    LecturerAcademicYearPlanId = plan.Id,
                    SourceType = giaItem.Item.SourceType,
                    SourceRowId = giaItem.Item.SourceRowId,
                    SourceAcademicPlanRecordId = giaItem.Item.SourceAcademicPlanRecordId,
                    LoadElementType = giaItem.Item.LoadElementType,
                    DistributionUnitType = DistributionUnitType.Students,
                    StudentGroupId = null,
                    ContingentSubgroupId = null,
                    UnitName = $"{giaItem.StudentsCount} студ.",
                    StudentsCount = giaItem.StudentsCount,
                    AssignedHours = giaItem.Hours
                });
            }

            await _context.SaveChangesAsync();

            return WorkloadDistributionOperationResult.Ok(
                "Нагрузка назначена преподавателю.",
                lecturerId);
        }

        public async Task<WorkloadDistributionOperationResult> DeleteAssignmentAsync(
            int selectedYearStart,
            int assignmentId)
        {
            var academicYear = AcademicYearResolver.BuildAcademicYear(
                AcademicYearResolver.NormalizeStartYear(selectedYearStart));

            var assignment = await _context.LecturerLoadAssignments
                .Include(x => x.LecturerAcademicYearPlan)
                .FirstOrDefaultAsync(x =>
                    x.Id == assignmentId &&
                    x.AcademicYear == academicYear);

            if (assignment == null)
            {
                return WorkloadDistributionOperationResult.Fail("Назначение не найдено.");
            }

            var lecturerId = assignment.LecturerAcademicYearPlan?.LecturerId;

            _context.LecturerLoadAssignments.Remove(assignment);

            await _context.SaveChangesAsync();

            return WorkloadDistributionOperationResult.Ok(
                "Назначение удалено.",
                lecturerId);
        }

        private async Task<List<DistributableLoadItem>> BuildDistributableItemsAsync(
            string academicYear,
            List<LecturerLoadAssignment> assignments)
        {
            var result = new List<DistributableLoadItem>();
            var norms = await LoadNormsAsync();

            var groupItems = await LoadStudentGroupsAsync();

            var subgroups = await _context.ContingentSubgroups
                .AsNoTracking()
                .OrderBy(x => x.StudentGroupId)
                .ThenBy(x => x.SubgroupNumber)
                .ToListAsync();

            var subgroupsByGroupId = subgroups
                .GroupBy(x => x.StudentGroupId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var disciplineRows = await _context.WorkloadRows
                .AsNoTracking()
                .Where(x => x.AcademicYear == academicYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.DisciplineName)
                .ThenBy(x => x.DirectionCode)
                .ToListAsync();

            var calculatedDisciplineRows = await _workloadCalculationService
                .BuildRowsForTableAsync(disciplineRows);

            foreach (var row in calculatedDisciplineRows)
            {
                var rowGroups = GetGroupsForRow(row, groupItems);

                AddDisciplineItems(
                    result,
                    row,
                    rowGroups,
                    subgroupsByGroupId,
                    LoadAssignmentElementType.Lecture,
                    "Лекции",
                    row.LecturePlanHours,
                    row.LectureTotalHours,
                    norms.GetValueOrDefault(LectureNormName),
                    forceGroupDistribution: false);

                AddDisciplineItems(
                    result,
                    row,
                    rowGroups,
                    subgroupsByGroupId,
                    LoadAssignmentElementType.Practice,
                    "Практические занятия",
                    row.PracticePlanHours,
                    row.PracticeTotalHours,
                    norms.GetValueOrDefault(PracticeNormName),
                    forceGroupDistribution: false);

                AddDisciplineItems(
                    result,
                    row,
                    rowGroups,
                    subgroupsByGroupId,
                    LoadAssignmentElementType.Laboratory,
                    "Лабораторные занятия",
                    row.LabPlanHours,
                    row.LabTotalHours,
                    norms.GetValueOrDefault(LabNormName),
                    forceGroupDistribution: false);

                AddControlGroupItems(
                    result,
                    row,
                    rowGroups,
                    LoadAssignmentElementType.Consultation,
                    "Консультации",
                    row.ConsultationHours);

                AddControlGroupItems(
                    result,
                    row,
                    rowGroups,
                    LoadAssignmentElementType.Exam,
                    "Экзамен",
                    row.ExamHours);

                AddControlGroupItems(
                    result,
                    row,
                    rowGroups,
                    LoadAssignmentElementType.Credit,
                    "Зачет",
                    row.CreditHours);

                AddControlGroupItems(
                    result,
                    row,
                    rowGroups,
                    LoadAssignmentElementType.CourseWork,
                    "Курсовая работа",
                    row.CourseWorkHours);

                AddControlGroupItems(
                    result,
                    row,
                    rowGroups,
                    LoadAssignmentElementType.CourseProject,
                    "Курсовой проект",
                    row.CourseProjectHours);
            }

            var practiceRows = await _context.PracticeWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == academicYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.PracticeName)
                .ToListAsync();

            await _practiceCalculationService.RecalculateAsync(practiceRows);

            foreach (var row in practiceRows)
            {
                var rowGroups = GetGroupsForDirectionAndCourse(
                    row.DirectionCode,
                    row.Course,
                    groupItems);

                AddPracticeGroupItems(result, row, rowGroups);
            }

            var giaRows = await _context.GiaWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == academicYear)
                .OrderBy(x => x.Course)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.GiaSection)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            await _giaCalculationService.RecalculateAsync(giaRows);

            foreach (var row in giaRows)
            {
                AddGiaStudentItem(result, row);
            }

            ApplyAssignedInfo(result, assignments);

            return result
                .OrderBy(x => x.SourceType)
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.ElementDisplayName)
                .ThenBy(x => x.UnitName)
                .ToList();
        }

        private void AddDisciplineItems(
            List<DistributableLoadItem> result,
            WorkloadRow row,
            List<StudentGroupItem> rowGroups,
            Dictionary<int, List<ContingentSubgroup>> subgroupsByGroupId,
            LoadAssignmentElementType elementType,
            string elementDisplayName,
            decimal planHours,
            decimal totalHours,
            NormTime? norm,
            bool forceGroupDistribution)
        {
            if (totalHours <= 0)
            {
                return;
            }

            var baseType = forceGroupDistribution
                ? WorkCalculationBase.PerGroup
                : norm?.CalculationBase ?? WorkCalculationBase.PerWork;

            if (baseType == WorkCalculationBase.PerStream)
            {
                result.Add(CreateItem(
                    row,
                    elementType,
                    DistributionUnitType.Flow,
                    elementDisplayName,
                    $"поток {row.DirectionCode}",
                    null,
                    null,
                    row.StudentsCount,
                    totalHours));
                return;
            }

            if (baseType == WorkCalculationBase.PerSubgroup)
            {
                foreach (var group in rowGroups)
                {
                    if (!subgroupsByGroupId.TryGetValue(group.Id, out var groupSubgroups) || !groupSubgroups.Any())
                    {
                        result.Add(CreateItem(
                            row,
                            elementType,
                            DistributionUnitType.Subgroup,
                            elementDisplayName,
                            $"гр. {group.GroupName} / 1 п/г",
                            group.Id,
                            null,
                            group.StudentCount,
                            CalculateSingleUnitHours(planHours, totalHours, Math.Max(1, row.SubgroupCount))));
                        continue;
                    }

                    foreach (var subgroup in groupSubgroups)
                    {
                        result.Add(CreateItem(
                            row,
                            elementType,
                            DistributionUnitType.Subgroup,
                            elementDisplayName,
                            $"гр. {group.GroupName} / {subgroup.SubgroupNumber} п/г",
                            group.Id,
                            subgroup.Id,
                            subgroup.StudentsCount,
                            CalculateSingleUnitHours(planHours, totalHours, Math.Max(1, row.SubgroupCount))));
                    }
                }

                return;
            }

            foreach (var group in rowGroups)
            {
                result.Add(CreateItem(
                    row,
                    elementType,
                    DistributionUnitType.Group,
                    elementDisplayName,
                    $"гр. {group.GroupName}",
                    group.Id,
                    null,
                    group.StudentCount,
                    CalculateSingleUnitHours(planHours, totalHours, Math.Max(1, row.GroupCount))));
            }
        }

        private void AddControlGroupItems(
        List<DistributableLoadItem> result,
        WorkloadRow row,
        List<StudentGroupItem> rowGroups,
        LoadAssignmentElementType elementType,
        string elementDisplayName,
        decimal totalHours)
        {
            if (totalHours <= 0 || rowGroups.Count == 0)
            {
                return;
            }

            var groupHours = SplitHoursByGroups(totalHours, rowGroups);

            foreach (var item in groupHours)
            {
                result.Add(CreateItem(
                    row,
                    elementType,
                    DistributionUnitType.Group,
                    elementDisplayName,
                    $"гр. {item.Group.GroupName}",
                    item.Group.Id,
                    null,
                    item.Group.StudentCount,
                    item.Hours));
            }
        }

        private void AddPracticeGroupItems(
     List<DistributableLoadItem> result,
     PracticeWorkloadRow row,
     List<StudentGroupItem> rowGroups)
        {
            if (row.TotalHours <= 0 || rowGroups.Count == 0)
            {
                return;
            }

            var groupHours = SplitHoursByGroups(row.TotalHours, rowGroups);

            foreach (var item in groupHours)
            {
                result.Add(new DistributableLoadItem
                {
                    SourceType = LoadAssignmentSourceType.Practice,
                    SourceRowId = row.Id,
                    SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                    LoadElementType = LoadAssignmentElementType.PracticeWork,
                    DistributionUnitType = DistributionUnitType.Group,
                    StudentGroupId = item.Group.Id,
                    ContingentSubgroupId = null,
                    SemesterName = row.SemesterName,
                    Title = row.PracticeName,
                    Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                    ElementDisplayName = "Практика",
                    UnitName = $"гр. {item.Group.GroupName}",
                    StudentsCount = item.Group.StudentCount,
                    TotalHours = item.Hours,
                    RemainingHours = item.Hours
                });
            }
        }

        private void AddGiaStudentItem(
       List<DistributableLoadItem> result,
       GiaWorkloadRow row)
        {
            if (row.TotalHours <= 0 || row.StudentsCount <= 0)
            {
                return;
            }

            var roundedTotalHours = RoundHours(row.TotalHours);
            var hoursPerStudent = row.TotalHours / row.StudentsCount;

            result.Add(new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Gia,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.GiaWork,
                DistributionUnitType = DistributionUnitType.Students,
                StudentGroupId = null,
                ContingentSubgroupId = null,
                SemesterName = row.SemesterName,
                Title = row.GiaSection,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = row.WorkName,
                UnitName = "студенты",
                StudentsCount = row.StudentsCount,
                TotalHours = roundedTotalHours,
                RemainingHours = roundedTotalHours,
                RemainingStudentsCount = row.StudentsCount,
                HoursPerStudent = hoursPerStudent,
                IsGiaStudentsInput = true
            });
        }
        private static DistributableLoadItem CreateItem(
       WorkloadRow row,
       LoadAssignmentElementType elementType,
       DistributionUnitType unitType,
       string elementDisplayName,
       string unitName,
       int? studentGroupId,
       int? subgroupId,
       int studentsCount,
       decimal hours)
        {
            var roundedHours = RoundHours(hours);

            return new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = elementType,
                DistributionUnitType = unitType,
                StudentGroupId = studentGroupId,
                ContingentSubgroupId = subgroupId,
                SemesterName = row.SemesterName,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = elementDisplayName,
                UnitName = unitName,
                StudentsCount = studentsCount,
                TotalHours = roundedHours,
                RemainingHours = roundedHours
            };
        }
        private static decimal CalculateSingleUnitHours(
       decimal planHours,
       decimal totalHours,
       int unitCount)
        {
            if (planHours > 0)
            {
                return RoundHours(planHours);
            }

            return unitCount > 0
                ? RoundHours(totalHours / unitCount)
                : RoundHours(totalHours);
        }
        private static void ApplyAssignedInfo(
            List<DistributableLoadItem> items,
            List<LecturerLoadAssignment> assignments)
        {
            foreach (var item in items)
            {
                var itemAssignments = assignments
                    .Where(x => BuildAssignmentKey(x) == BuildItemKey(item))
                    .ToList();

                item.AssignedHours = itemAssignments.Sum(x => x.AssignedHours);
                item.RemainingHours = Math.Max(0, item.TotalHours - item.AssignedHours);

                if (item.SourceType == LoadAssignmentSourceType.Gia)
                {
                    var assignedStudents = itemAssignments.Sum(x => x.StudentsCount);
                    item.RemainingStudentsCount = Math.Max(0, item.StudentsCount - assignedStudents);
                    item.RemainingHours = Math.Round(item.RemainingStudentsCount * item.HoursPerStudent, 2, MidpointRounding.AwayFromZero);
                }
            }
        }

        private async Task<Dictionary<string, NormTime>> LoadNormsAsync()
        {
            var norms = await _context.NormTimes
                .AsNoTracking()
                .ToListAsync();

            return norms
                .Where(x => !string.IsNullOrWhiteSpace(x.WorkName))
                .GroupBy(x => x.WorkName)
                .ToDictionary(x => x.Key, x => x.First());
        }

        private async Task<List<StudentGroupItem>> LoadStudentGroupsAsync()
        {
            return await (
                from studentGroup in _context.StudentGroupsCore.AsNoTracking()
                join direction in _context.EducationDirections.AsNoTracking()
                    on studentGroup.EducationDirectionId equals direction.Id
                select new StudentGroupItem
                {
                    Id = studentGroup.Id,
                    GroupName = studentGroup.GroupName,
                    StudentCount = studentGroup.StudentCount,
                    Course = (int)studentGroup.Course,
                    DirectionCode = direction.Cipher
                })
                .ToListAsync();
        }

        private static List<StudentGroupItem> GetGroupsForRow(
            WorkloadRow row,
            List<StudentGroupItem> groups)
        {
            var directionCodes = SplitDirectionCodes(row.DirectionCode);

            return groups
                .Where(x => directionCodes.Contains(TextNormalizeHelper.Normalize(x.DirectionCode)))
                .Where(x => x.Course == row.Course)
                .OrderBy(x => x.GroupName)
                .ToList();
        }

        private static List<StudentGroupItem> GetGroupsForDirectionAndCourse(
            string directionCode,
            int course,
            List<StudentGroupItem> groups)
        {
            var normalizedDirectionCode = TextNormalizeHelper.Normalize(directionCode);

            return groups
                .Where(x => TextNormalizeHelper.Normalize(x.DirectionCode) == normalizedDirectionCode)
                .Where(x => x.Course == course)
                .OrderBy(x => x.GroupName)
                .ToList();
        }

        private static List<string> SplitDirectionCodes(string directionCode)
        {
            return directionCode
                .Split(new[] { ',', '/', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(TextNormalizeHelper.Normalize)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        private static List<WorkloadDistributionSemesterGroupViewModel> BuildSemesterGroups(
            List<DistributableLoadItem> items)
        {
            return items
                .Where(x => x.SourceType != LoadAssignmentSourceType.Gia)
                .Where(x => x.RemainingHours > 0)
                .GroupBy(x => x.SemesterName)
                .OrderBy(x => GetSemesterOrder(x.Key))
                .Select(semesterGroup => new WorkloadDistributionSemesterGroupViewModel
                {
                    SemesterName = semesterGroup.Key,
                    Disciplines = semesterGroup
                        .GroupBy(x => new { x.Title, x.Subtitle })
                        .OrderBy(x => x.Key.Title)
                        .Select(disciplineGroup => new WorkloadDistributionDisciplineGroupViewModel
                        {
                            Title = disciplineGroup.Key.Title,
                            Subtitle = disciplineGroup.Key.Subtitle,
                            WorkTypes = disciplineGroup
                                .GroupBy(x => x.ElementDisplayName)
                                .OrderBy(x => x.Key)
                                .Select(workTypeGroup => new WorkloadDistributionWorkTypeGroupViewModel
                                {
                                    ElementDisplayName = workTypeGroup.Key,
                                    Items = workTypeGroup
                                        .OrderBy(x => x.UnitName)
                                        .Select(MapAvailableItem)
                                        .ToList()
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();
        }

        private static List<WorkloadDistributionGiaItemViewModel> BuildGiaItems(
            List<DistributableLoadItem> items)
        {
            return items
                .Where(x => x.SourceType == LoadAssignmentSourceType.Gia)
                .Where(x => x.RemainingStudentsCount > 0)
                .OrderBy(x => x.SemesterName)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.ElementDisplayName)
                .Select(x => new WorkloadDistributionGiaItemViewModel
                {
                    ItemKey = BuildItemKey(x),
                    Title = x.Title,
                    Subtitle = x.Subtitle,
                    ElementDisplayName = x.ElementDisplayName,
                    RemainingStudentsCount = x.RemainingStudentsCount,
                    HoursPerStudent = x.HoursPerStudent
                })
                .ToList();
        }

        private async Task EnsureAcademicYearPlansAsync(string academicYear)
        {
            var existingLecturerIds = await _context.LecturerAcademicYearPlans
                .Where(x => x.AcademicYear == academicYear)
                .Select(x => x.LecturerId)
                .ToListAsync();

            var missingLecturers = await _context.Lecturers
                .AsNoTracking()
                .Where(x => !existingLecturerIds.Contains(x.Id))
                .ToListAsync();

            if (missingLecturers.Count == 0)
            {
                return;
            }

            foreach (var lecturer in missingLecturers)
            {
                _context.LecturerAcademicYearPlans.Add(new LecturerAcademicYearPlan
                {
                    AcademicYear = academicYear,
                    LecturerId = lecturer.Id,
                    LecturerStudyPostId = lecturer.LecturerStudyPostId,
                    Rate = 1.00m
                });
            }

            await _context.SaveChangesAsync();
        }

        private static WorkloadDistributionAvailableItemViewModel MapAvailableItem(
            DistributableLoadItem item)
        {
            return new WorkloadDistributionAvailableItemViewModel
            {
                ItemKey = BuildItemKey(item),
                SourceTypeDisplayName = GetSourceDisplayName(item.SourceType),
                SemesterName = item.SemesterName,
                Title = item.Title,
                Subtitle = item.Subtitle,
                ElementDisplayName = item.ElementDisplayName,
                UnitName = item.UnitName,
                TotalHours = item.TotalHours,
                AssignedHours = item.AssignedHours,
                RemainingHours = item.RemainingHours,
                StudentsCount = item.StudentsCount,
                IsGiaStudentsInput = item.IsGiaStudentsInput,
                RemainingStudentsCount = item.RemainingStudentsCount,
                HoursPerStudent = item.HoursPerStudent
            };
        }

        private static decimal CalculateLimitHours(int normHours, decimal rate)
        {
            return Math.Round(normHours * rate, 0, MidpointRounding.AwayFromZero);
        }

        private static bool IsAssistant(string? studyPostTitle)
        {
            return !string.IsNullOrWhiteSpace(studyPostTitle)
                   && studyPostTitle
                       .Trim()
                       .ToLowerInvariant()
                       .Contains(AssistantPostKeyword);
        }

        private static decimal RoundHours(decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        private static int RoundHoursToInt(decimal value)
        {
            return (int)Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        private static List<GroupHoursItem> SplitHoursByGroups(
            decimal totalHours,
            List<StudentGroupItem> groups)
        {
            var result = new List<GroupHoursItem>();

            if (groups.Count == 0)
            {
                return result;
            }

            var totalHoursInt = RoundHoursToInt(totalHours);

            if (totalHoursInt <= 0)
            {
                return result;
            }

            var totalStudents = groups.Sum(x => x.StudentCount);

            if (totalStudents <= 0)
            {
                var baseHours = totalHoursInt / groups.Count;
                var remainder = totalHoursInt % groups.Count;

                for (var i = 0; i < groups.Count; i++)
                {
                    result.Add(new GroupHoursItem
                    {
                        Group = groups[i],
                        Hours = baseHours + (i < remainder ? 1 : 0)
                    });
                }

                return result;
            }

            var calculated = groups
                .Select(group =>
                {
                    var exactHours = (decimal)totalHoursInt * group.StudentCount / totalStudents;
                    var floorHours = (int)Math.Floor(exactHours);

                    return new
                    {
                        Group = group,
                        FloorHours = floorHours,
                        Fraction = exactHours - floorHours
                    };
                })
                .ToList();

            var distributedHours = calculated.Sum(x => x.FloorHours);
            var remainderHours = totalHoursInt - distributedHours;

            var groupsWithRemainder = calculated
                .OrderByDescending(x => x.Fraction)
                .ThenBy(x => x.Group.GroupName)
                .ToList();

            var extraByGroupId = new Dictionary<int, int>();

            for (var i = 0; i < remainderHours; i++)
            {
                var group = groupsWithRemainder[i % groupsWithRemainder.Count].Group;

                if (!extraByGroupId.ContainsKey(group.Id))
                {
                    extraByGroupId[group.Id] = 0;
                }

                extraByGroupId[group.Id]++;
            }

            foreach (var item in calculated)
            {
                result.Add(new GroupHoursItem
                {
                    Group = item.Group,
                    Hours = item.FloorHours + extraByGroupId.GetValueOrDefault(item.Group.Id)
                });
            }

            return result;
        }
        private static int GetSemesterOrder(string semesterName)
        {
            return semesterName.Equals("Осень", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
        }

        private static string BuildItemKey(DistributableLoadItem item)
        {
            return string.Join("_", new[]
            {
                ((int)item.SourceType).ToString(),
                item.SourceRowId.ToString(),
                item.SourceAcademicPlanRecordId.ToString(),
                ((int)item.LoadElementType).ToString(),
                ((int)item.DistributionUnitType).ToString(),
                (item.StudentGroupId ?? 0).ToString(),
                (item.ContingentSubgroupId ?? 0).ToString()
            });
        }

        private static string BuildAssignmentKey(LecturerLoadAssignment assignment)
        {
            return string.Join("_", new[]
            {
                ((int)assignment.SourceType).ToString(),
                assignment.SourceRowId.ToString(),
                assignment.SourceAcademicPlanRecordId.ToString(),
                ((int)assignment.LoadElementType).ToString(),
                ((int)assignment.DistributionUnitType).ToString(),
                (assignment.StudentGroupId ?? 0).ToString(),
                (assignment.ContingentSubgroupId ?? 0).ToString()
            });
        }

        private static bool TryParseKey(
            string key,
            out ParsedAssignmentKey parsed)
        {
            parsed = new ParsedAssignmentKey();

            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var parts = key.Split('_');

            if (parts.Length != 7)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var sourceTypeValue)
                || !int.TryParse(parts[1], out var sourceRowId)
                || !int.TryParse(parts[2], out var recordId)
                || !int.TryParse(parts[3], out var elementTypeValue)
                || !int.TryParse(parts[4], out var unitTypeValue)
                || !int.TryParse(parts[5], out var groupId)
                || !int.TryParse(parts[6], out var subgroupId))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(LoadAssignmentSourceType), sourceTypeValue)
                || !Enum.IsDefined(typeof(LoadAssignmentElementType), elementTypeValue)
                || !Enum.IsDefined(typeof(DistributionUnitType), unitTypeValue))
            {
                return false;
            }

            parsed = new ParsedAssignmentKey
            {
                SourceType = (LoadAssignmentSourceType)sourceTypeValue,
                SourceRowId = sourceRowId,
                SourceAcademicPlanRecordId = recordId,
                LoadElementType = (LoadAssignmentElementType)elementTypeValue,
                DistributionUnitType = (DistributionUnitType)unitTypeValue,
                StudentGroupId = groupId > 0 ? groupId : null,
                ContingentSubgroupId = subgroupId > 0 ? subgroupId : null
            };

            return true;
        }

        private static string GetLecturerDisplayName(Lecturer? lecturer)
        {
            if (lecturer == null)
            {
                return string.Empty;
            }

            return string.Join(" ", new[]
            {
                lecturer.LastName,
                lecturer.FirstName,
                lecturer.Patronymic
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static string GetSourceDisplayName(LoadAssignmentSourceType sourceType)
        {
            return sourceType switch
            {
                LoadAssignmentSourceType.Discipline => "Дисциплина",
                LoadAssignmentSourceType.Practice => "Практика",
                LoadAssignmentSourceType.Gia => "ГИА",
                _ => "Неизвестно"
            };
        }

        private sealed class StudentGroupItem
        {
            public int Id { get; set; }

            public string GroupName { get; set; } = string.Empty;

            public int StudentCount { get; set; }

            public int Course { get; set; }

            public string DirectionCode { get; set; } = string.Empty;
        }
        private sealed class GroupHoursItem
        {
            public StudentGroupItem Group { get; set; } = new();

            public decimal Hours { get; set; }
        }
        private sealed class ParsedAssignmentKey
        {
            public LoadAssignmentSourceType SourceType { get; set; }

            public int SourceRowId { get; set; }

            public int SourceAcademicPlanRecordId { get; set; }

            public LoadAssignmentElementType LoadElementType { get; set; }

            public DistributionUnitType DistributionUnitType { get; set; }

            public int? StudentGroupId { get; set; }

            public int? ContingentSubgroupId { get; set; }
        }

        private sealed class DistributableLoadItem
        {
            public LoadAssignmentSourceType SourceType { get; set; }

            public int SourceRowId { get; set; }

            public int SourceAcademicPlanRecordId { get; set; }

            public LoadAssignmentElementType LoadElementType { get; set; }

            public DistributionUnitType DistributionUnitType { get; set; }

            public int? StudentGroupId { get; set; }

            public int? ContingentSubgroupId { get; set; }

            public string SemesterName { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;

            public string Subtitle { get; set; } = string.Empty;

            public string ElementDisplayName { get; set; } = string.Empty;

            public string UnitName { get; set; } = string.Empty;

            public int StudentsCount { get; set; }

            public decimal TotalHours { get; set; }

            public decimal AssignedHours { get; set; }

            public decimal RemainingHours { get; set; }

            public bool IsGiaStudentsInput { get; set; }

            public int RemainingStudentsCount { get; set; }

            public decimal HoursPerStudent { get; set; }
        }
    }

    public sealed class WorkloadDistributionOperationResult
    {
        public bool Success { get; private set; }

        public string Message { get; private set; } = string.Empty;

        public int? LecturerId { get; private set; }

        public static WorkloadDistributionOperationResult Ok(
            string message,
            int? lecturerId = null)
        {
            return new WorkloadDistributionOperationResult
            {
                Success = true,
                Message = message,
                LecturerId = lecturerId
            };
        }

        public static WorkloadDistributionOperationResult Fail(
            string message,
            int? lecturerId = null)
        {
            return new WorkloadDistributionOperationResult
            {
                Success = false,
                Message = message,
                LecturerId = lecturerId
            };
        }
    }
}