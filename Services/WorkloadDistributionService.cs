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
using DepartmentLoadApp.Models.AdditionalWork;

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

        private readonly DepartmentLoadDbContext _context;
        private readonly WorkloadCalculationService _workloadCalculationService;
        private readonly PracticeCalculationService _practiceCalculationService;
        private readonly GiaCalculationService _giaCalculationService;
        private readonly AdditionalWorkCalculationService _additionalWorkCalculationService;

        public WorkloadDistributionService(
             DepartmentLoadDbContext context,
             WorkloadCalculationService workloadCalculationService,
             PracticeCalculationService practiceCalculationService,
             GiaCalculationService giaCalculationService,
             AdditionalWorkCalculationService additionalWorkCalculationService)
        {
            _context = context;
            _workloadCalculationService = workloadCalculationService;
            _practiceCalculationService = practiceCalculationService;
            _giaCalculationService = giaCalculationService;
            _additionalWorkCalculationService = additionalWorkCalculationService;
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
                .AsNoTracking()
                .Where(x => x.AcademicYear == academicYear)
                .ToListAsync();

            var items = await BuildDistributableItemsAsync(academicYear, assignments);

            var validAssignments = assignments
                .Where(x => FindItemForAssignment(items, x) != null)
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
                RemainingHours = Math.Max(
                    0,
                    items.Sum(x => x.TotalHours) - validAssignments.Sum(x => x.AssignedHours)),
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
                    .OrderBy(x => GetSemesterSortOrder(x.SemesterName))
                    .ThenBy(x => x.SemesterName)
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
                    .OrderBy(x => GetSemesterSortOrder(x.SemesterName))
                    .ThenBy(x => x.SemesterName)
                    .ThenBy(x => x.Title)
                    .ThenBy(x => x.ElementDisplayName)
                    .ThenBy(x => x.UnitName)
                    .ToList();

                card.AvailableItems = availableForLecturer
                    .Select(MapAvailableItem)
                    .ToList();

                card.SemesterGroups = BuildSemesterGroups(availableForLecturer);
                card.GiaItems = BuildGiaItems(availableForLecturer);
                card.AdditionalWorkItems = BuildAdditionalWorkItems(availableForLecturer);

                foreach (var assignment in lecturerAssignments)
                {
                    var item = FindItemForAssignment(items, assignment);

                    if (item == null)
                    {
                        continue;
                    }

                    card.Assignments.Add(new WorkloadDistributionAssignmentViewModel
                    {
                        AssignmentId = assignment.Id,
                        SourceTypeDisplayName = GetSourceDisplayName(assignment.SourceType),
                        SemesterName = item.SemesterName,
                        Title = item.Title,
                        Subtitle = item.Subtitle,
                        ElementDisplayName = item.ElementDisplayName,
                        UnitName = string.IsNullOrWhiteSpace(assignment.UnitName)
                            ? item.UnitName
                            : assignment.UnitName,
                        StudentsCount = assignment.StudentsCount > 0
                            ? assignment.StudentsCount
                            : item.StudentsCount,
                        AssignedHours = assignment.AssignedHours,
                        TotalItemHours = item.TotalHours,
                        RemainingItemHours = item.RemainingHours
                    });
                }

                card.Assignments = card.Assignments
                    .OrderBy(x => x.SourceTypeDisplayName)
                    .ThenBy(x => x.SemesterName)
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

            var newStudyPost = await _context.LecturerStudyPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == lecturerStudyPostId.Value);

            if (newStudyPost == null)
            {
                return WorkloadDistributionOperationResult.Fail(
                    "Учебная должность не найдена.",
                    lecturerId);
            }

            var currentAssignedHours = await _context.LecturerLoadAssignments
                .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
                .SumAsync(x => (decimal?)x.AssignedHours) ?? 0m;

            var newLimitHours = CalculateLimitHours(newStudyPost.Hours, normalizedRate);

            if (currentAssignedHours > newLimitHours)
            {
                return WorkloadDistributionOperationResult.Fail(
                    $"Нельзя сохранить: у преподавателя уже назначено {currentAssignedHours:0.##} ч., а по новой ставке и должности можно только {newLimitHours:0.##} ч.",
                    lecturerId);
            }

            if (IsAssistant(newStudyPost.StudyPostTitle))
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
           List<GiaStudentsAssignmentInputModel> giaStudents,
           List<AdditionalWorkAssignmentInputModel> additionalWorks)
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
            additionalWorks ??= new List<AdditionalWorkAssignmentInputModel>();

            var yearAssignments = await _context.LecturerLoadAssignments
                .AsNoTracking()
                .Where(x => x.AcademicYear == academicYear)
                .ToListAsync();

            var availableItems = await BuildDistributableItemsAsync(academicYear, yearAssignments);

            var selectedItems = new List<DistributableLoadItem>();

            foreach (var itemKey in selectedItemKeys.Distinct())
            {
                if (!TryParseKey(itemKey, out _))
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
                if (item.SourceType == LoadAssignmentSourceType.Gia ||
                    item.SourceType == LoadAssignmentSourceType.AdditionalWork)
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
            var additionalSelectedItems = new List<(DistributableLoadItem Item, int StudentsCount, decimal Hours)>();

            foreach (var input in additionalWorks.Where(x => x.StudentsCount > 0 || x.Hours > 0))
            {
                var item = availableItems.FirstOrDefault(x => BuildItemKey(x) == input.ItemKey);

                if (item == null || item.SourceType != LoadAssignmentSourceType.AdditionalWork)
                {
                    return WorkloadDistributionOperationResult.Fail(
                        "Некорректная строка доп. работы.",
                        lecturerId);
                }

                if (item.LoadElementType == LoadAssignmentElementType.PostgraduateSupervision)
                {
                    if (input.StudentsCount <= 0)
                    {
                        continue;
                    }

                    if (input.StudentsCount > item.RemainingStudentsCount)
                    {
                        return WorkloadDistributionOperationResult.Fail(
                            $"По строке «{item.ElementDisplayName}» осталось только {item.RemainingStudentsCount} аспирантов.",
                            lecturerId);
                    }

                    if (item.HoursPerStudent <= 0)
                    {
                        return WorkloadDistributionOperationResult.Fail(
                            $"Для строки «{item.ElementDisplayName}» не задана норма часов.",
                            lecturerId);
                    }

                    var hours = RoundHours(input.StudentsCount * item.HoursPerStudent);

                    if (hours <= 0)
                    {
                        continue;
                    }

                    additionalSelectedItems.Add((item, input.StudentsCount, hours));

                    continue;
                }

                if (item.LoadElementType == LoadAssignmentElementType.OrganizationalWork)
                {
                    var hours = RoundHours(input.Hours);

                    if (hours <= 0)
                    {
                        continue;
                    }

                    if (hours > item.RemainingHours)
                    {
                        return WorkloadDistributionOperationResult.Fail(
                            $"По строке «{item.ElementDisplayName}» осталось только {item.RemainingHours:0.##} ч.",
                            lecturerId);
                    }

                    additionalSelectedItems.Add((item, 0, hours));
                }
            }
            if (!selectedItems.Any() && !giaSelectedItems.Any() && !additionalSelectedItems.Any())
            {
                return WorkloadDistributionOperationResult.Fail(
                    "Выберите хотя бы один элемент нагрузки.",
                    lecturerId);
            }

            var lecturerAssignedHours = yearAssignments
                .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
                .Sum(x => x.AssignedHours);

            var limitHours = CalculateLimitHours(plan.LecturerStudyPost?.Hours ?? 0, plan.Rate);

            var selectedHours = selectedItems.Sum(x => x.RemainingHours)
                                + giaSelectedItems.Sum(x => x.Hours)
                                + additionalSelectedItems.Sum(x => x.Hours);

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
            foreach (var additionalItem in additionalSelectedItems)
            {
                var unitName = additionalItem.Item.LoadElementType == LoadAssignmentElementType.PostgraduateSupervision
                    ? $"{additionalItem.StudentsCount} асп."
                    : $"{additionalItem.Hours:0.##} ч.";

                _context.LecturerLoadAssignments.Add(new LecturerLoadAssignment
                {
                    AcademicYear = academicYear,
                    LecturerAcademicYearPlanId = plan.Id,
                    SourceType = additionalItem.Item.SourceType,
                    SourceRowId = additionalItem.Item.SourceRowId,
                    SourceAcademicPlanRecordId = additionalItem.Item.SourceAcademicPlanRecordId,
                    LoadElementType = additionalItem.Item.LoadElementType,
                    DistributionUnitType = DistributionUnitType.Students,
                    StudentGroupId = null,
                    ContingentSubgroupId = null,
                    UnitName = unitName,
                    StudentsCount = additionalItem.StudentsCount,
                    AssignedHours = additionalItem.Hours
                });
            }
            await _context.SaveChangesAsync();

            return WorkloadDistributionOperationResult.Ok(
                "Нагрузка назначена преподавателю.",
                lecturerId);
        }

        public async Task<WorkloadDistributionOperationResult> ClearAssignmentsForYearAsync(
    int selectedYearStart)
        {
            var academicYear = AcademicYearResolver.BuildAcademicYear(
                AcademicYearResolver.NormalizeStartYear(selectedYearStart));

            var assignments = await _context.LecturerLoadAssignments
                .Where(x => x.AcademicYear == academicYear)
                .ToListAsync();

            if (assignments.Count == 0)
            {
                return WorkloadDistributionOperationResult.Ok(
                    $"За {academicYear} учебный год распределений для очистки нет.");
            }

            _context.LecturerLoadAssignments.RemoveRange(assignments);

            await _context.SaveChangesAsync();

            return WorkloadDistributionOperationResult.Ok(
                $"Распределения за {academicYear} учебный год очищены. Удалено назначений: {assignments.Count}.");
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

        public async Task<AutoDistributionResultViewModel> AutoDistributeByHistoryAsync(int selectedYearStart)
        {
            var targetYearStart = AcademicYearResolver.NormalizeStartYear(selectedYearStart);
            var targetAcademicYear = AcademicYearResolver.BuildAcademicYear(targetYearStart);

            var historyAcademicYears = new List<string>
    {
        AcademicYearResolver.BuildAcademicYear(targetYearStart - 1),
        AcademicYearResolver.BuildAcademicYear(targetYearStart - 2)
    };

            var result = new AutoDistributionResultViewModel
            {
                IsSuccess = true,
                TargetAcademicYear = targetAcademicYear,
                HistoryAcademicYears = historyAcademicYears
            };

            await EnsureAcademicYearPlansAsync(targetAcademicYear);

            var currentAssignments = await _context.LecturerLoadAssignments
                .AsNoTracking()
                .Where(x => x.AcademicYear == targetAcademicYear)
                .ToListAsync();

            var currentItems = await BuildDistributableItemsAsync(targetAcademicYear, currentAssignments);

            var currentRows = await _context.WorkloadRows
                .AsNoTracking()
                .Where(x => x.AcademicYear == targetAcademicYear)
                .ToListAsync();

            var currentRowsById = currentRows.ToDictionary(x => x.Id);

            var lectureHistory = await BuildLectureHistoryAsync(historyAcademicYears);

            if (lectureHistory.Count == 0)
            {
                result.IsSuccess = false;
                result.Message = "Не найдена история распределения лекций за прошлые годы.";

                return result;
            }

            var currentPlans = await _context.LecturerAcademicYearPlans
                .Include(x => x.Lecturer)
                .Include(x => x.LecturerStudyPost)
                .Where(x => x.AcademicYear == targetAcademicYear)
                .ToListAsync();

            var currentPlansByLecturerId = currentPlans
                .GroupBy(x => x.LecturerId)
                .ToDictionary(x => x.Key, x => x.First());

            var assignedHoursByPlanId = currentAssignments
                .GroupBy(x => x.LecturerAcademicYearPlanId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Sum(a => a.AssignedHours));

            var addedAssignments = new List<LecturerLoadAssignment>();
            var createdInfos = new List<AutoCreatedAssignmentInfo>();

            var disciplineGroups = currentItems
                .Where(x => x.SourceType == LoadAssignmentSourceType.Discipline)
                .Where(x => IsAutoDistributionElement(x.LoadElementType))
                .Where(x => x.RemainingHours > 0)
                .GroupBy(x => BuildCurrentAutoFlowKey(x, currentRowsById))
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .OrderBy(x => x.First().Title)
                .ToList();

            foreach (var disciplineGroup in disciplineGroups)
            {
                if (!lectureHistory.TryGetValue(disciplineGroup.Key, out var historyCandidates)
                    || historyCandidates.Count == 0)
                {
                    continue;
                }

                var candidate = historyCandidates
                    .FirstOrDefault(x => currentPlansByLecturerId.ContainsKey(x.LecturerId));

                if (candidate == null)
                {
                    continue;
                }

                var plan = currentPlansByLecturerId[candidate.LecturerId];

                if (IsAssistant(plan.LecturerStudyPost?.StudyPostTitle))
                {
                    continue;
                }

                var lecturerName = GetLecturerDisplayName(plan.Lecturer);

                foreach (var item in disciplineGroup
                             .OrderBy(x => GetAutoElementSortOrder(x.LoadElementType))
                             .ThenBy(x => x.UnitName))
                {
                    if (item.RemainingHours <= 0)
                    {
                        continue;
                    }

                    var itemKey = BuildItemKey(item);

                    var alreadyAssignedToThisLecturer = currentAssignments
                        .Concat(addedAssignments)
                        .Any(x =>
                            x.LecturerAcademicYearPlanId == plan.Id &&
                            BuildAssignmentKey(x) == itemKey);

                    if (alreadyAssignedToThisLecturer)
                    {
                        continue;
                    }

                    var lecturerAssignedHours = assignedHoursByPlanId.GetValueOrDefault(plan.Id);
                    var lecturerLimitHours = CalculateLimitHours(plan.LecturerStudyPost?.Hours ?? 0, plan.Rate);
                    var lecturerFreeHours = lecturerLimitHours - lecturerAssignedHours;

                    if (lecturerFreeHours < item.RemainingHours)
                    {
                        continue;
                    }

                    var assignment = new LecturerLoadAssignment
                    {
                        AcademicYear = targetAcademicYear,
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
                    };

                    _context.LecturerLoadAssignments.Add(assignment);
                    addedAssignments.Add(assignment);

                    assignedHoursByPlanId[plan.Id] = lecturerAssignedHours + item.RemainingHours;

                    createdInfos.Add(new AutoCreatedAssignmentInfo
                    {
                        Assignment = assignment,
                        LecturerId = plan.LecturerId,
                        LecturerName = lecturerName,
                        GroupKey = disciplineGroup.Key,
                        DisciplineName = item.Title,
                        Subtitle = item.Subtitle,
                        SemesterName = item.SemesterName,
                        ElementName = item.ElementDisplayName,
                        UnitName = item.UnitName,
                        Hours = item.RemainingHours
                    });

                    item.AssignedHours += item.RemainingHours;
                    item.RemainingHours = 0;
                }
            }

            if (addedAssignments.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            result.CreatedAssignmentsCount = addedAssignments.Count;
            result.Groups = createdInfos
                .GroupBy(x => new
                {
                    x.LecturerId,
                    x.LecturerName,
                    x.DisciplineName,
                    x.GroupKey
                })
                .OrderBy(x => x.Key.LecturerName)
                .ThenBy(x => x.Key.DisciplineName)
                .Select(x => new AutoDistributionGroupViewModel
                {
                    LecturerId = x.Key.LecturerId,
                    LecturerName = x.Key.LecturerName,
                    DisciplineName = x.Key.DisciplineName,
                    Subtitle = BuildAutoDistributionGroupSubtitle(x),
                    TotalHours = x.Sum(a => a.Hours),
                    Assignments = x
                        .OrderBy(a => GetAutoElementNameSortOrder(a.ElementName))
                        .ThenBy(a => a.UnitName)
                        .Select(a => new AutoDistributionAssignmentViewModel
                        {
                            AssignmentId = a.Assignment.Id,
                            ElementName = a.ElementName,
                            UnitName = a.UnitName,
                            Hours = a.Hours
                        })
                        .ToList()
                })
                .ToList();

            result.Message = addedAssignments.Count > 0
                ? $"Автоматически распределено назначений: {addedAssignments.Count}."
                : "Автораспределение выполнено, но новых назначений не создано.";

            return result;
        }

        private async Task<Dictionary<string, List<LectureHistoryCandidate>>> BuildLectureHistoryAsync(
            List<string> historyAcademicYears)
        {
            var historyAssignments = await _context.LecturerLoadAssignments
                .AsNoTracking()
                .Include(x => x.LecturerAcademicYearPlan)
                .ThenInclude(x => x!.Lecturer)
                .Where(x => historyAcademicYears.Contains(x.AcademicYear))
                .Where(x => x.SourceType == LoadAssignmentSourceType.Discipline)
                .Where(x => x.LoadElementType == LoadAssignmentElementType.Lecture)
                .ToListAsync();

            if (historyAssignments.Count == 0)
            {
                return new Dictionary<string, List<LectureHistoryCandidate>>();
            }

            var sourceRowIds = historyAssignments
                .Select(x => x.SourceRowId)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var sourceAcademicPlanRecordIds = historyAssignments
                .Select(x => x.SourceAcademicPlanRecordId)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var historyRows = await _context.WorkloadRows
                .AsNoTracking()
                .Where(x => historyAcademicYears.Contains(x.AcademicYear))
                .Where(x =>
                    sourceRowIds.Contains(x.Id) ||
                    sourceAcademicPlanRecordIds.Contains(x.AcademicPlanRecordId))
                .ToListAsync();

            var rowsById = historyRows
                .GroupBy(x => x.Id)
                .ToDictionary(x => x.Key, x => x.First());

            var rowsByAcademicPlanRecordId = historyRows
                .Where(x => x.AcademicPlanRecordId > 0)
                .GroupBy(x => BuildHistoryAcademicPlanRecordKey(x.AcademicYear, x.AcademicPlanRecordId))
                .ToDictionary(x => x.Key, x => x.First());

            var candidates = new List<LectureHistoryCandidate>();

            foreach (var assignment in historyAssignments)
            {
                if (assignment.LecturerAcademicYearPlan == null)
                {
                    continue;
                }

                WorkloadRow? row = null;

                if (assignment.SourceRowId > 0)
                {
                    rowsById.TryGetValue(assignment.SourceRowId, out row);
                }

                if (row == null && assignment.SourceAcademicPlanRecordId > 0)
                {
                    rowsByAcademicPlanRecordId.TryGetValue(
                        BuildHistoryAcademicPlanRecordKey(
                            assignment.AcademicYear,
                            assignment.SourceAcademicPlanRecordId),
                        out row);
                }

                if (row == null)
                {
                    continue;
                }

                candidates.Add(new LectureHistoryCandidate
                {
                    Key = BuildAutoFlowKey(row),
                    LecturerId = assignment.LecturerAcademicYearPlan.LecturerId,
                    AcademicYear = assignment.AcademicYear,
                    AcademicYearStart = ExtractAcademicYearStart(assignment.AcademicYear),
                    Hours = assignment.AssignedHours
                });
            }

            return candidates
                .GroupBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .OrderByDescending(c => c.AcademicYearStart)
                        .ThenByDescending(c => c.Hours)
                        .ToList());
        }

        private static string BuildCurrentAutoFlowKey(
    DistributableLoadItem item,
    Dictionary<int, WorkloadRow> currentRowsById)
        {
            if (currentRowsById.TryGetValue(item.SourceRowId, out var row))
            {
                return BuildAutoFlowKey(row);
            }

            return BuildAutoFlowKey(
                item.Title,
                ExtractCourseNumberFromSubtitle(item.Subtitle),
                item.SemesterName);
        }

        private static string BuildAutoFlowKey(WorkloadRow row)
        {
            return BuildAutoFlowKey(
                row.DisciplineName,
                row.Course,
                row.SemesterName);
        }

        private static string BuildAutoFlowKey(
            string? disciplineName,
            int course,
            string? semesterName)
        {
            return string.Join("|", new[]
            {
        NormalizeAutoKeyPart(disciplineName),
        course.ToString(),
        NormalizeAutoKeyPart(semesterName)
    });
        }

        private static int ExtractCourseNumberFromSubtitle(string? subtitle)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                return 0;
            }

            var coursePart = ExtractCoursePart(subtitle);

            var digits = new string(coursePart
                .Where(char.IsDigit)
                .ToArray());

            return int.TryParse(digits, out var course)
                ? course
                : 0;
        }

        private static string BuildAutoDistributionGroupSubtitle(
            IEnumerable<AutoCreatedAssignmentInfo> infos)
        {
            var list = infos.ToList();

            if (list.Count == 0)
            {
                return string.Empty;
            }

            var directions = list
                .SelectMany(x => ExtractDirectionParts(x.Subtitle))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var coursePart = list
                .Select(x => ExtractCoursePart(x.Subtitle))
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            var semesterName = list
                .Select(x => x.SemesterName)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            return string.Join(" · ", new[]
            {
        directions.Count > 0 ? string.Join(", ", directions) : null,
        coursePart,
        semesterName
    }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static string BuildCurrentAutoDisciplineKey(
            DistributableLoadItem item,
            Dictionary<int, WorkloadRow> currentRowsById)
        {
            if (currentRowsById.TryGetValue(item.SourceRowId, out var row))
            {
                return BuildAutoDisciplineKey(
                    row.DisciplineName,
                    row.DirectionCode,
                    row.SemesterName);
            }

            return BuildAutoDisciplineKey(
                item.Title,
                ExtractDirectionCodeFromSubtitle(item.Subtitle),
                item.SemesterName);
        }

        private static bool IsAutoDistributionElement(LoadAssignmentElementType elementType)
        {
            return elementType == LoadAssignmentElementType.Lecture
                   || elementType == LoadAssignmentElementType.Consultation
                   || elementType == LoadAssignmentElementType.Exam
                   || elementType == LoadAssignmentElementType.Credit
                   || elementType == LoadAssignmentElementType.CourseWork
                   || elementType == LoadAssignmentElementType.CourseProject
                   || elementType == LoadAssignmentElementType.Rgr;
        }

        private static int GetAutoElementSortOrder(LoadAssignmentElementType elementType)
        {
            return elementType switch
            {
                LoadAssignmentElementType.Lecture => 1,
                LoadAssignmentElementType.Consultation => 2,
                LoadAssignmentElementType.Exam => 3,
                LoadAssignmentElementType.Credit => 4,
                LoadAssignmentElementType.CourseWork => 5,
                LoadAssignmentElementType.CourseProject => 6,
                LoadAssignmentElementType.Rgr => 7,
                _ => 100
            };
        }

        private static int GetAutoElementNameSortOrder(string elementName)
        {
            var value = NormalizeAutoKeyPart(elementName);

            return value switch
            {
                "лекции" => 1,
                "консультации" => 2,
                "экзамен" => 3,
                "зачет" => 4,
                "курсовая работа" => 5,
                "курсовой проект" => 6,
                "ргр" => 7,
                _ => 100
            };
        }

        private static string BuildAutoDisciplineKey(
            string? disciplineName,
            string? directionCode,
            string? semesterName)
        {
            return string.Join("|", new[]
            {
        NormalizeAutoKeyPart(disciplineName),
        NormalizeAutoKeyPart(directionCode),
        NormalizeAutoKeyPart(semesterName)
    });
        }

        private static string NormalizeAutoKeyPart(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant().Replace("ё", "е");
        }

        private static string ExtractDirectionCodeFromSubtitle(string? subtitle)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                return string.Empty;
            }

            return subtitle
                .Split('·', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()
                ?.Trim() ?? string.Empty;
        }

        private static int ExtractAcademicYearStart(string academicYear)
        {
            if (string.IsNullOrWhiteSpace(academicYear))
            {
                return 0;
            }

            var firstPart = academicYear
                .Split('-', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            return int.TryParse(firstPart, out var year)
                ? year
                : 0;
        }

        private static string BuildHistoryAcademicPlanRecordKey(
            string academicYear,
            int academicPlanRecordId)
        {
            return $"{academicYear}_{academicPlanRecordId}";
        }

        private sealed class LectureHistoryCandidate
        {
            public string Key { get; set; } = string.Empty;

            public int LecturerId { get; set; }

            public string AcademicYear { get; set; } = string.Empty;

            public int AcademicYearStart { get; set; }

            public decimal Hours { get; set; }
        }

        private sealed class AutoCreatedAssignmentInfo
        {
            public LecturerLoadAssignment Assignment { get; set; } = null!;

            public int LecturerId { get; set; }

            public string LecturerName { get; set; } = string.Empty;

            public string GroupKey { get; set; } = string.Empty;

            public string DisciplineName { get; set; } = string.Empty;

            public string Subtitle { get; set; } = string.Empty;

            public string SemesterName { get; set; } = string.Empty;

            public string ElementName { get; set; } = string.Empty;

            public string UnitName { get; set; } = string.Empty;

            public decimal Hours { get; set; }
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

            var lectureRows = await _workloadCalculationService
                .BuildRowsForTableAsync(disciplineRows);

            foreach (var row in lectureRows)
            {
                AddDisciplineItems(
                    result,
                    row,
                    GetGroupsForRow(row, groupItems),
                    subgroupsByGroupId,
                    LoadAssignmentElementType.Lecture,
                    "Лекции",
                    row.LecturePlanHours,
                    row.LectureTotalHours,
                    norms.GetValueOrDefault(LectureNormName),
                    forceGroupDistribution: false);
            }

            foreach (var row in disciplineRows)
            {
                var rowGroups = GetGroupsForRow(row, groupItems);

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

                AddControlGroupItems(
                    result,
                    row,
                    rowGroups,
                    LoadAssignmentElementType.Rgr,
                    "РГР",
                    row.RgrHours);
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

            var additionalWorkItems = await _additionalWorkCalculationService
     .BuildDistributionItemsAsync(academicYear);

            foreach (var row in additionalWorkItems)
            {
                var isPostgraduate = row.WorkType == AdditionalWorkType.PostgraduateSupervision;

                result.Add(new DistributableLoadItem
                {
                    SourceType = LoadAssignmentSourceType.AdditionalWork,
                    SourceRowId = row.SourceRowId,
                    SourceAcademicPlanRecordId = row.SourceAcademicPlanRecordId,
                    LoadElementType = row.LoadElementType,
                    DistributionUnitType = isPostgraduate
                        ? DistributionUnitType.Students
                        : DistributionUnitType.Flow,
                    StudentGroupId = null,
                    ContingentSubgroupId = null,
                    SemesterName = "Доп. работа",
                    Title = row.Title,
                    Subtitle = row.Subtitle,
                    ElementDisplayName = row.ElementDisplayName,
                    UnitName = isPostgraduate ? "аспиранты" : "часы",
                    StudentsCount = isPostgraduate ? row.Count : 0,
                    TotalHours = row.TotalHours,
                    RemainingHours = row.TotalHours,
                    TotalStudentsCount = isPostgraduate ? row.Count : 0,
                    RemainingStudentsCount = isPostgraduate ? row.Count : 0,
                    HoursPerStudent = isPostgraduate ? row.HoursPerUnit : 0
                });
            }

            ApplyAssignedInfo(result, assignments);

            return result
                .OrderBy(x => x.SourceType)
                .ThenBy(x => GetSemesterSortOrder(x.SemesterName))
                .ThenBy(x => x.SemesterName)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.ElementDisplayName)
                .ThenBy(x => x.UnitName)
                .ToList();
        }

        private void AddDisciplineItems(
            List<DistributableLoadItem> result,
            WorkloadRow row,
            List<StudentGroupDistributionItem> rowGroups,
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
                    if (!subgroupsByGroupId.TryGetValue(group.Id, out var groupSubgroups) ||
                        !groupSubgroups.Any())
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
            List<StudentGroupDistributionItem> rowGroups,
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
            List<StudentGroupDistributionItem> rowGroups)
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
                TotalHours = row.TotalHours,
                RemainingHours = row.TotalHours,
                TotalStudentsCount = row.StudentsCount,
                RemainingStudentsCount = row.StudentsCount,
                HoursPerStudent = hoursPerStudent
            });
        }

        private DistributableLoadItem CreateItem(
            WorkloadRow row,
            LoadAssignmentElementType elementType,
            DistributionUnitType distributionUnitType,
            string elementDisplayName,
            string unitName,
            int? studentGroupId,
            int? contingentSubgroupId,
            int studentsCount,
            decimal totalHours)
        {
            totalHours = RoundHours(totalHours);

            return new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = elementType,
                DistributionUnitType = distributionUnitType,
                StudentGroupId = studentGroupId,
                ContingentSubgroupId = contingentSubgroupId,
                SemesterName = row.SemesterName,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = elementDisplayName,
                UnitName = unitName,
                StudentsCount = studentsCount,
                TotalHours = totalHours,
                RemainingHours = totalHours
            };
        }

        private static void ApplyAssignedInfo(
       List<DistributableLoadItem> items,
       List<LecturerLoadAssignment> assignments)
        {
            foreach (var item in items)
            {
                item.AssignedHours = 0;
                item.RemainingHours = item.TotalHours;

                if (item.SourceType == LoadAssignmentSourceType.Gia ||
                    item.LoadElementType == LoadAssignmentElementType.PostgraduateSupervision)
                {
                    item.AssignedStudentsCount = 0;
                    item.RemainingStudentsCount = item.TotalStudentsCount;
                }
            }

            foreach (var assignment in assignments)
            {
                var item = FindItemForAssignment(items, assignment);

                if (item == null)
                {
                    continue;
                }

                item.AssignedHours += assignment.AssignedHours;
                item.RemainingHours = Math.Max(0, item.TotalHours - item.AssignedHours);

                if (item.SourceType == LoadAssignmentSourceType.Gia ||
     item.LoadElementType == LoadAssignmentElementType.PostgraduateSupervision)
                {
                    item.AssignedStudentsCount += assignment.StudentsCount;
                    item.RemainingStudentsCount = Math.Max(
                        0,
                        item.TotalStudentsCount - item.AssignedStudentsCount);
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
                .GroupBy(
                    x => x.WorkName.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    x => x.Key,
                    x => x.First(),
                    StringComparer.OrdinalIgnoreCase);
        }

        private async Task<List<StudentGroupDistributionItem>> LoadStudentGroupsAsync()
        {
            var groups = await (
                    from groupItem in _context.StudentGroupsCore.AsNoTracking()
                    join direction in _context.EducationDirections.AsNoTracking()
                        on groupItem.EducationDirectionId equals direction.Id
                    select new StudentGroupDistributionItem
                    {
                        Id = groupItem.Id,
                        DirectionCode = direction.Cipher,
                        Course = (int)groupItem.Course,
                        GroupName = groupItem.GroupName,
                        StudentCount = groupItem.StudentCount
                    })
                .OrderBy(x => x.DirectionCode)
                .ThenBy(x => x.Course)
                .ThenBy(x => x.GroupName)
                .ToListAsync();

            return groups;
        }

        private static List<StudentGroupDistributionItem> GetGroupsForRow(
            WorkloadRow row,
            List<StudentGroupDistributionItem> groups)
        {
            return GetGroupsForDirectionAndCourse(row.DirectionCode, row.Course, groups);
        }

        private static List<StudentGroupDistributionItem> GetGroupsForDirectionAndCourse(
            string directionCode,
            int course,
            List<StudentGroupDistributionItem> groups)
        {
            return groups
                .Where(x => string.Equals(
                    x.DirectionCode,
                    directionCode,
                    StringComparison.OrdinalIgnoreCase))
                .Where(x => x.Course == course)
                .OrderBy(x => x.GroupName)
                .ToList();
        }

        private static List<(StudentGroupDistributionItem Group, decimal Hours)> SplitHoursByGroups(
     decimal totalHours,
     List<StudentGroupDistributionItem> groups)
        {
            var result = new List<(StudentGroupDistributionItem Group, decimal Hours)>();

            if (totalHours <= 0 || groups.Count == 0)
            {
                return result;
            }

            var roundedTotalHours = (int)RoundHours(totalHours);

            if (roundedTotalHours <= 0)
            {
                return result;
            }

            var baseHours = roundedTotalHours / groups.Count;
            var remainder = roundedTotalHours % groups.Count;

            for (var i = 0; i < groups.Count; i++)
            {
                var hours = baseHours + (i < remainder ? 1 : 0);

                if (hours <= 0)
                {
                    continue;
                }

                result.Add((groups[i], hours));
            }

            return result;
        }
        private static decimal CalculateSingleUnitHours(
     decimal planHours,
     decimal totalHours,
     int unitsCount)
        {
            if (planHours > 0)
            {
                return RoundHours(planHours);
            }

            if (unitsCount <= 0)
            {
                return RoundHours(totalHours);
            }

            return RoundHours(totalHours / unitsCount);
        }
        private static List<WorkloadDistributionSemesterGroupViewModel> BuildSemesterGroups(
      List<DistributableLoadItem> items)
        {
            return items
                .Where(x => x.SourceType != LoadAssignmentSourceType.Gia)
                .Where(x => x.SourceType != LoadAssignmentSourceType.AdditionalWork)
                .GroupBy(x => x.SemesterName)
                .OrderBy(x => GetSemesterSortOrder(x.Key))
                .ThenBy(x => x.Key)
                .Select(semesterGroup => new WorkloadDistributionSemesterGroupViewModel
                {
                    SemesterName = semesterGroup.Key,
                    Disciplines = semesterGroup
                        .GroupBy(BuildDisciplineAccordionKey)
                        .OrderBy(x => x.First().Title)
                        .Select(disciplineGroup => new WorkloadDistributionDisciplineGroupViewModel
                        {
                            Title = disciplineGroup.First().Title,
                            Subtitle = BuildDisciplineAccordionSubtitle(disciplineGroup),
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
                .OrderBy(x => x.Title)
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

        private static decimal CalculateLimitHours(int normHours, decimal rate)
        {
            return Math.Round(normHours * rate, 2, MidpointRounding.AwayFromZero);
        }

        private static decimal RoundHours(decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        private static bool IsAssistant(string? studyPostTitle)
        {
            return !string.IsNullOrWhiteSpace(studyPostTitle)
                   && studyPostTitle
                       .Trim()
                       .ToLowerInvariant()
                       .Contains(AssistantPostKeyword);
        }

        private static string BuildItemKey(DistributableLoadItem item)
        {
            return BuildKey(
                item.SourceType,
                item.SourceRowId,
                item.LoadElementType,
                item.DistributionUnitType,
                item.StudentGroupId,
                item.ContingentSubgroupId);
        }

        private static string BuildAssignmentKey(LecturerLoadAssignment assignment)
        {
            return BuildKey(
                assignment.SourceType,
                assignment.SourceRowId,
                assignment.LoadElementType,
                assignment.DistributionUnitType,
                assignment.StudentGroupId,
                assignment.ContingentSubgroupId);
        }

        private static DistributableLoadItem? FindItemForAssignment(
    List<DistributableLoadItem> items,
    LecturerLoadAssignment assignment)
        {
            var exactKey = BuildAssignmentKey(assignment);

            var exactItem = items.FirstOrDefault(x => BuildItemKey(x) == exactKey);

            if (exactItem != null)
            {
                return exactItem;
            }

            return items.FirstOrDefault(x => IsBroadAssignmentMatch(x, assignment));
        }

        private static bool IsBroadAssignmentMatch(
            DistributableLoadItem item,
            LecturerLoadAssignment assignment)
        {
            if (item.SourceType != assignment.SourceType)
            {
                return false;
            }

            if (item.LoadElementType != assignment.LoadElementType)
            {
                return false;
            }

            if (assignment.SourceRowId > 0 && item.SourceRowId == assignment.SourceRowId)
            {
                return true;
            }

            if (assignment.SourceAcademicPlanRecordId > 0 &&
                item.SourceAcademicPlanRecordId == assignment.SourceAcademicPlanRecordId)
            {
                return true;
            }

            return false;
        }
        private static int GetSemesterSortOrder(string? semesterName)
        {
            if (string.IsNullOrWhiteSpace(semesterName))
            {
                return 99;
            }

            var value = semesterName.Trim().ToLowerInvariant();

            if (value.Contains("осень") ||
                value.Contains("осен") ||
                value.Contains("1 сем") ||
                value.Contains("1-й сем") ||
                value.Contains("семестр 1"))
            {
                return 1;
            }

            if (value.Contains("весна") ||
                value.Contains("весен") ||
                value.Contains("2 сем") ||
                value.Contains("2-й сем") ||
                value.Contains("семестр 2"))
            {
                return 2;
            }

            return 50;
        }
        private static string BuildKey(
            LoadAssignmentSourceType sourceType,
            int sourceRowId,
            LoadAssignmentElementType elementType,
            DistributionUnitType distributionUnitType,
            int? studentGroupId,
            int? contingentSubgroupId)
        {
            return string.Join("_", new[]
            {
                ((int)sourceType).ToString(),
                sourceRowId.ToString(),
                ((int)elementType).ToString(),
                ((int)distributionUnitType).ToString(),
                studentGroupId?.ToString() ?? "0",
                contingentSubgroupId?.ToString() ?? "0"
            });
        }

        private static bool TryParseKey(
            string key,
            out ParsedLoadItemKey parsed)
        {
            parsed = new ParsedLoadItemKey();

            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var parts = key.Split('_');

            if (parts.Length != 6)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var sourceTypeValue))
            {
                return false;
            }

            if (!int.TryParse(parts[1], out var sourceRowId))
            {
                return false;
            }

            if (!int.TryParse(parts[2], out var elementTypeValue))
            {
                return false;
            }

            if (!int.TryParse(parts[3], out var distributionUnitTypeValue))
            {
                return false;
            }

            if (!int.TryParse(parts[4], out var studentGroupIdValue))
            {
                return false;
            }

            if (!int.TryParse(parts[5], out var contingentSubgroupIdValue))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(LoadAssignmentSourceType), sourceTypeValue))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(LoadAssignmentElementType), elementTypeValue))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(DistributionUnitType), distributionUnitTypeValue))
            {
                return false;
            }

            parsed = new ParsedLoadItemKey
            {
                SourceType = (LoadAssignmentSourceType)sourceTypeValue,
                SourceRowId = sourceRowId,
                LoadElementType = (LoadAssignmentElementType)elementTypeValue,
                DistributionUnitType = (DistributionUnitType)distributionUnitTypeValue,
                StudentGroupId = studentGroupIdValue == 0 ? null : studentGroupIdValue,
                ContingentSubgroupId = contingentSubgroupIdValue == 0 ? null : contingentSubgroupIdValue
            };

            return true;
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
                IsGiaStudentsInput = item.SourceType == LoadAssignmentSourceType.Gia,
                RemainingStudentsCount = item.RemainingStudentsCount,
                HoursPerStudent = item.HoursPerStudent
            };
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
                LoadAssignmentSourceType.AdditionalWork => "Доп. работа",
                _ => "Неизвестно"
            };
        }
        private static string BuildDisciplineAccordionKey(DistributableLoadItem item)
        {
            return string.Join("|", new[]
            {
        NormalizeAccordionKey(item.Title),
        NormalizeAccordionKey(ExtractCoursePart(item.Subtitle)),
        NormalizeAccordionKey(item.SemesterName)
    });
        }

        private static string BuildDisciplineAccordionSubtitle(
            IEnumerable<DistributableLoadItem> items)
        {
            var itemList = items.ToList();

            if (itemList.Count == 0)
            {
                return string.Empty;
            }

            var first = itemList[0];

            var directions = itemList
                .SelectMany(x => ExtractDirectionParts(x.Subtitle))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var coursePart = ExtractCoursePart(first.Subtitle);
            var semesterPart = first.SemesterName;

            return string.Join(" · ", new[]
            {
        directions.Count > 0 ? string.Join(", ", directions) : null,
        coursePart,
        semesterPart
    }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static List<string> ExtractDirectionParts(string? subtitle)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                return new List<string>();
            }

            var firstPart = subtitle
                .Split('·', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(firstPart))
            {
                return new List<string>();
            }

            return firstPart
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private static string ExtractCoursePart(string? subtitle)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                return string.Empty;
            }

            return subtitle
                .Split('·', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .FirstOrDefault(x => x.Contains("курс", StringComparison.OrdinalIgnoreCase))
                ?? string.Empty;
        }

        private static string NormalizeAccordionKey(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }

        private static List<WorkloadDistributionAvailableItemViewModel> BuildAdditionalWorkItems(List<DistributableLoadItem> items)
        {
            return items
                .Where(x => x.SourceType == LoadAssignmentSourceType.AdditionalWork)
                .Where(x => x.RemainingHours > 0)
                .OrderBy(x => x.Title)
                .ThenBy(x => x.ElementDisplayName)
                .Select(MapAvailableItem)
                .ToList();
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

            public int TotalStudentsCount { get; set; }

            public int AssignedStudentsCount { get; set; }

            public int RemainingStudentsCount { get; set; }

            public decimal HoursPerStudent { get; set; }
        }

        private sealed class StudentGroupDistributionItem
        {
            public int Id { get; set; }

            public string DirectionCode { get; set; } = string.Empty;

            public int Course { get; set; }

            public string GroupName { get; set; } = string.Empty;

            public int StudentCount { get; set; }
        }

        private sealed class ParsedLoadItemKey
        {
            public LoadAssignmentSourceType SourceType { get; set; }

            public int SourceRowId { get; set; }

            public LoadAssignmentElementType LoadElementType { get; set; }

            public DistributionUnitType DistributionUnitType { get; set; }

            public int? StudentGroupId { get; set; }

            public int? ContingentSubgroupId { get; set; }
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