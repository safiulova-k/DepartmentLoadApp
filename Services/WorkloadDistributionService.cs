using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Core;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.ViewModels.WorkloadDistribution;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services;

public class WorkloadDistributionService
{
    private readonly DepartmentLoadDbContext _context;

    public WorkloadDistributionService(DepartmentLoadDbContext context)
    {
        _context = context;
    }

    public async Task<WorkloadDistributionPageViewModel> BuildPageAsync(int? startYear, int? selectedLecturerId = null)
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
            .GroupBy(x => BuildKey(x.SourceType, x.SourceAcademicPlanRecordId, x.LoadElementType))
            .ToDictionary(g => g.Key, g => g.First());

        var validAssignments = assignments
            .Where(x => x.SourceAcademicPlanRecordId > 0)
            .Where(x => itemMap.ContainsKey(BuildKey(x.SourceType, x.SourceAcademicPlanRecordId, x.LoadElementType)))
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
            StudyPosts = studyPosts.Select(x => new WorkloadDistributionStudyPostOptionViewModel
            {
                Id = x.Id,
                Title = x.StudyPostTitle,
                NormHours = x.Hours
            }).ToList(),
            RemainingItems = items
                .Where(x => x.RemainingHours > 0)
                .OrderBy(x => x.SourceType)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.ElementDisplayName)
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

            var existingKeys = lecturerAssignments
                .Select(x => BuildKey(x.SourceType, x.SourceAcademicPlanRecordId, x.LoadElementType))
                .ToHashSet();

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

            card.AvailableItems = items
                .Where(x => x.RemainingHours > 0)
                .Where(x => !existingKeys.Contains(BuildKey(x.SourceType, x.SourceAcademicPlanRecordId, x.LoadElementType)))
                .Where(x => !(isAssistant && x.LoadElementType == LoadAssignmentElementType.Lecture))
                .OrderBy(x => x.SourceType)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.ElementDisplayName)
                .Select(MapAvailableItem)
                .ToList();

            foreach (var assignment in lecturerAssignments)
            {
                var key = BuildKey(
                    assignment.SourceType,
                    assignment.SourceAcademicPlanRecordId,
                    assignment.LoadElementType);

                if (!itemMap.TryGetValue(key, out var item))
                {
                    continue;
                }

                var maxForItem = item.TotalHours - (item.AssignedHours - assignment.AssignedHours);
                var maxForLecturer = limitHours - (assignedHours - assignment.AssignedHours);
                var maxAllowed = Math.Max(0, Math.Min(maxForItem, maxForLecturer));

                card.Assignments.Add(new WorkloadDistributionAssignmentViewModel
                {
                    AssignmentId = assignment.Id,
                    SourceTypeDisplayName = GetSourceDisplayName(assignment.SourceType),
                    Title = item.Title,
                    Subtitle = item.Subtitle,
                    ElementDisplayName = item.ElementDisplayName,
                    AssignedHours = assignment.AssignedHours,
                    TotalItemHours = item.TotalHours,
                    RemainingItemHours = Math.Max(0, item.TotalHours - item.AssignedHours),
                    CanIncrease = assignment.AssignedHours < maxAllowed,
                    CanDecrease = assignment.AssignedHours > 0
                });
            }

            card.Assignments = card.Assignments
                .OrderBy(x => x.SourceTypeDisplayName)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.ElementDisplayName)
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

        if (normalizedRate < 0 || normalizedRate > 2.00m)
        {
            return WorkloadDistributionOperationResult.Fail(
                "Ставка должна быть в диапазоне от 0 до 2.00.",
                lecturerId);
        }

        var plan = await _context.LecturerAcademicYearPlans
            .Include(x => x.LecturerStudyPost)
            .FirstOrDefaultAsync(x => x.AcademicYear == academicYear && x.LecturerId == lecturerId);

        if (plan == null)
        {
            return WorkloadDistributionOperationResult.Fail("План преподавателя не найден.", lecturerId);
        }

        LecturerStudyPost? newStudyPost = null;

        if (lecturerStudyPostId.HasValue)
        {
            newStudyPost = await _context.LecturerStudyPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == lecturerStudyPostId.Value);

            if (newStudyPost == null)
            {
                return WorkloadDistributionOperationResult.Fail("Учебная должность не найдена.", lecturerId);
            }
        }

        var currentAssignedHours = await _context.LecturerLoadAssignments
            .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
            .SumAsync(x => (int?)x.AssignedHours) ?? 0;

        var newLimitHours = CalculateLimitHours(newStudyPost?.Hours ?? 0, normalizedRate);

        if (currentAssignedHours > newLimitHours)
        {
            return WorkloadDistributionOperationResult.Fail(
                $"Нельзя сохранить: у преподавателя уже назначено {currentAssignedHours} ч., а по новой ставке и должности можно только {newLimitHours} ч.",
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

        return WorkloadDistributionOperationResult.Ok("Параметры преподавателя сохранены.", lecturerId);
    }

    public async Task<WorkloadDistributionOperationResult> AddAssignmentAsync(
        int selectedYearStart,
        int lecturerId,
        string itemKey)
    {
        var academicYear = AcademicYearResolver.BuildAcademicYear(
            AcademicYearResolver.NormalizeStartYear(selectedYearStart));

        await EnsureAcademicYearPlansAsync(academicYear);

        if (!TryParseKey(itemKey, out var sourceType, out var sourceAcademicPlanRecordId, out var elementType))
        {
            return WorkloadDistributionOperationResult.Fail("Некорректный элемент нагрузки.", lecturerId);
        }

        var plan = await _context.LecturerAcademicYearPlans
            .Include(x => x.LecturerStudyPost)
            .FirstOrDefaultAsync(x => x.AcademicYear == academicYear && x.LecturerId == lecturerId);

        if (plan == null)
        {
            return WorkloadDistributionOperationResult.Fail("План преподавателя не найден.", lecturerId);
        }

        if (IsAssistant(plan.LecturerStudyPost?.StudyPostTitle) &&
            elementType == LoadAssignmentElementType.Lecture)
        {
            return WorkloadDistributionOperationResult.Fail("Ассистенту нельзя назначать лекции.", lecturerId);
        }

        var alreadyExists = await _context.LecturerLoadAssignments.AnyAsync(x =>
            x.AcademicYear == academicYear &&
            x.LecturerAcademicYearPlanId == plan.Id &&
            x.SourceType == sourceType &&
            x.SourceAcademicPlanRecordId == sourceAcademicPlanRecordId &&
            x.LoadElementType == elementType);

        if (alreadyExists)
        {
            return WorkloadDistributionOperationResult.Fail(
                "Этот элемент уже назначен выбранному преподавателю.",
                lecturerId);
        }

        var assignments = await _context.LecturerLoadAssignments
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .ToListAsync();

        var items = await BuildDistributableItemsAsync(academicYear, assignments);

        var item = items.FirstOrDefault(x =>
            x.SourceType == sourceType &&
            x.SourceAcademicPlanRecordId == sourceAcademicPlanRecordId &&
            x.LoadElementType == elementType);

        if (item == null)
        {
            return WorkloadDistributionOperationResult.Fail("Элемент нагрузки не найден.", lecturerId);
        }

        if (item.RemainingHours <= 0)
        {
            return WorkloadDistributionOperationResult.Fail(
                "По этому элементу больше нет свободных часов.",
                lecturerId);
        }

        var lecturerAssignedHours = assignments
            .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
            .Sum(x => x.AssignedHours);

        var limitHours = CalculateLimitHours(plan.LecturerStudyPost?.Hours ?? 0, plan.Rate);
        var lecturerRemainingHours = limitHours - lecturerAssignedHours;

        if (lecturerRemainingHours <= 0)
        {
            return WorkloadDistributionOperationResult.Fail(
                "У преподавателя уже исчерпан лимит часов.",
                lecturerId);
        }

        var initialHours = Math.Min(1, Math.Min(item.RemainingHours, lecturerRemainingHours));

        if (initialHours <= 0)
        {
            return WorkloadDistributionOperationResult.Fail("Назначить часы не удалось.", lecturerId);
        }

        _context.LecturerLoadAssignments.Add(new LecturerLoadAssignment
        {
            AcademicYear = academicYear,
            LecturerAcademicYearPlanId = plan.Id,
            SourceType = sourceType,
            SourceRowId = item.SourceRowId,
            SourceAcademicPlanRecordId = sourceAcademicPlanRecordId,
            LoadElementType = elementType,
            AssignedHours = initialHours
        });

        await _context.SaveChangesAsync();

        return WorkloadDistributionOperationResult.Ok("Нагрузка назначена преподавателю.", lecturerId);
    }

    public async Task<WorkloadDistributionOperationResult> ChangeAssignmentHoursAsync(
        int selectedYearStart,
        int assignmentId,
        int delta)
    {
        var academicYear = AcademicYearResolver.BuildAcademicYear(
            AcademicYearResolver.NormalizeStartYear(selectedYearStart));

        var assignment = await _context.LecturerLoadAssignments
            .Include(x => x.LecturerAcademicYearPlan)
            .ThenInclude(x => x!.LecturerStudyPost)
            .FirstOrDefaultAsync(x => x.Id == assignmentId && x.AcademicYear == academicYear);

        if (assignment == null || assignment.LecturerAcademicYearPlan == null)
        {
            return WorkloadDistributionOperationResult.Fail("Назначение не найдено.");
        }

        var lecturerId = assignment.LecturerAcademicYearPlan.LecturerId;

        var itemTotalHours = await GetItemTotalHoursAsync(
            academicYear,
            assignment.SourceType,
            assignment.SourceAcademicPlanRecordId,
            assignment.LoadElementType);

        if (itemTotalHours <= 0)
        {
            _context.LecturerLoadAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return WorkloadDistributionOperationResult.Ok(
                "Исходная нагрузка больше не существует. Назначение удалено.",
                lecturerId);
        }

        var yearAssignments = await _context.LecturerLoadAssignments
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .ToListAsync();

        var sameItemAssignedHours = yearAssignments
            .Where(x => x.Id != assignment.Id)
            .Where(x => x.SourceType == assignment.SourceType)
            .Where(x => x.SourceAcademicPlanRecordId == assignment.SourceAcademicPlanRecordId)
            .Where(x => x.LoadElementType == assignment.LoadElementType)
            .Sum(x => x.AssignedHours);

        var lecturerOtherAssignedHours = yearAssignments
            .Where(x => x.Id != assignment.Id)
            .Where(x => x.LecturerAcademicYearPlanId == assignment.LecturerAcademicYearPlanId)
            .Sum(x => x.AssignedHours);

        var limitHours = CalculateLimitHours(
            assignment.LecturerAcademicYearPlan.LecturerStudyPost?.Hours ?? 0,
            assignment.LecturerAcademicYearPlan.Rate);

        var newHours = assignment.AssignedHours + delta;

        if (newHours <= 0)
        {
            _context.LecturerLoadAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return WorkloadDistributionOperationResult.Ok("Назначение удалено.", lecturerId);
        }

        var maxByItem = itemTotalHours - sameItemAssignedHours;
        var maxByLecturer = limitHours - lecturerOtherAssignedHours;
        var maxAllowed = Math.Max(0, Math.Min(maxByItem, maxByLecturer));

        if (newHours > maxAllowed)
        {
            return WorkloadDistributionOperationResult.Fail(
                "Нельзя назначить больше часов по этому элементу.",
                lecturerId);
        }

        assignment.AssignedHours = newHours;
        await _context.SaveChangesAsync();

        return WorkloadDistributionOperationResult.Ok("Часы изменены.", lecturerId);
    }

    public async Task<WorkloadDistributionOperationResult> FillAssignmentToMaxAsync(
        int selectedYearStart,
        int assignmentId)
    {
        var academicYear = AcademicYearResolver.BuildAcademicYear(
            AcademicYearResolver.NormalizeStartYear(selectedYearStart));

        var assignment = await _context.LecturerLoadAssignments
            .Include(x => x.LecturerAcademicYearPlan)
            .ThenInclude(x => x!.LecturerStudyPost)
            .FirstOrDefaultAsync(x => x.Id == assignmentId && x.AcademicYear == academicYear);

        if (assignment == null || assignment.LecturerAcademicYearPlan == null)
        {
            return WorkloadDistributionOperationResult.Fail("Назначение не найдено.");
        }

        var lecturerId = assignment.LecturerAcademicYearPlan.LecturerId;

        var itemTotalHours = await GetItemTotalHoursAsync(
            academicYear,
            assignment.SourceType,
            assignment.SourceAcademicPlanRecordId,
            assignment.LoadElementType);

        if (itemTotalHours <= 0)
        {
            return WorkloadDistributionOperationResult.Fail("Элемент нагрузки не найден.", lecturerId);
        }

        var yearAssignments = await _context.LecturerLoadAssignments
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .ToListAsync();

        var sameItemAssignedHoursWithoutCurrent = yearAssignments
            .Where(x => x.Id != assignment.Id)
            .Where(x => x.SourceType == assignment.SourceType)
            .Where(x => x.SourceAcademicPlanRecordId == assignment.SourceAcademicPlanRecordId)
            .Where(x => x.LoadElementType == assignment.LoadElementType)
            .Sum(x => x.AssignedHours);

        var lecturerAssignedHoursWithoutCurrent = yearAssignments
            .Where(x => x.Id != assignment.Id)
            .Where(x => x.LecturerAcademicYearPlanId == assignment.LecturerAcademicYearPlanId)
            .Sum(x => x.AssignedHours);

        var limitHours = CalculateLimitHours(
            assignment.LecturerAcademicYearPlan.LecturerStudyPost?.Hours ?? 0,
            assignment.LecturerAcademicYearPlan.Rate);

        var maxByItem = itemTotalHours - sameItemAssignedHoursWithoutCurrent;
        var maxByLecturer = limitHours - lecturerAssignedHoursWithoutCurrent;
        var maxAllowed = Math.Max(0, Math.Min(maxByItem, maxByLecturer));

        if (maxAllowed <= 0)
        {
            return WorkloadDistributionOperationResult.Fail(
                "Нельзя назначить больше часов по этому элементу.",
                lecturerId);
        }

        assignment.AssignedHours = maxAllowed;
        await _context.SaveChangesAsync();

        return WorkloadDistributionOperationResult.Ok("Часы доведены до остатка.", lecturerId);
    }

    public async Task<WorkloadDistributionOperationResult> DeleteAssignmentAsync(
        int selectedYearStart,
        int assignmentId)
    {
        var academicYear = AcademicYearResolver.BuildAcademicYear(
            AcademicYearResolver.NormalizeStartYear(selectedYearStart));

        var assignment = await _context.LecturerLoadAssignments
            .Include(x => x.LecturerAcademicYearPlan)
            .FirstOrDefaultAsync(x => x.Id == assignmentId && x.AcademicYear == academicYear);

        if (assignment == null)
        {
            return WorkloadDistributionOperationResult.Fail("Назначение не найдено.");
        }

        var lecturerId = assignment.LecturerAcademicYearPlan?.LecturerId;

        _context.LecturerLoadAssignments.Remove(assignment);
        await _context.SaveChangesAsync();

        return WorkloadDistributionOperationResult.Ok("Назначение удалено.", lecturerId);
    }

    private async Task<List<DistributableLoadItem>> BuildDistributableItemsAsync(
        string academicYear,
        List<LecturerLoadAssignment> assignments)
    {
        var rawItems = new List<DistributableLoadItem>();

        var disciplineRows = await _context.WorkloadRows
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .OrderBy(x => x.Course)
            .ThenBy(x => x.SemesterName)
            .ThenBy(x => x.DisciplineName)
            .ToListAsync();

        foreach (var row in disciplineRows)
        {
            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.Lecture,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Лекции",
                TotalHours = RoundHoursToInt(row.LectureTotalHours)
            });

            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.Practice,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Практические занятия",
                TotalHours = RoundHoursToInt(row.PracticeTotalHours)
            });

            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.Laboratory,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Лабораторные занятия",
                TotalHours = RoundHoursToInt(row.LabTotalHours)
            });

            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.Consultation,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Консультации",
                TotalHours = RoundHoursToInt(row.ConsultationHours)
            });

            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.Exam,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Экзамен",
                TotalHours = RoundHoursToInt(row.ExamHours)
            });

            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.Credit,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Зачет",
                TotalHours = RoundHoursToInt(row.CreditHours)
            });

            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.CourseWork,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Курсовая работа",
                TotalHours = RoundHoursToInt(row.CourseWorkHours)
            });

            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Discipline,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.CourseProject,
                Title = row.DisciplineName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Курсовой проект",
                TotalHours = RoundHoursToInt(row.CourseProjectHours)
            });
        }

        var practiceRows = await _context.PracticeWorkloadRows
            .AsNoTracking()
            .Where(x => x.PlanYear == academicYear)
            .OrderBy(x => x.Course)
            .ThenBy(x => x.SemesterName)
            .ThenBy(x => x.PracticeName)
            .ToListAsync();

        foreach (var row in practiceRows)
        {
            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Practice,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.PracticeWork,
                Title = row.PracticeName,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = "Практика",
                TotalHours = RoundHoursToInt(row.TotalHours)
            });
        }

        var giaRows = await _context.GiaWorkloadRows
            .AsNoTracking()
            .Where(x => x.PlanYear == academicYear)
            .OrderBy(x => x.Course)
            .ThenBy(x => x.SemesterName)
            .ThenBy(x => x.GiaSection)
            .ThenBy(x => x.WorkName)
            .ToListAsync();

        foreach (var row in giaRows)
        {
            AddItemIfNeeded(rawItems, new DistributableLoadItem
            {
                SourceType = LoadAssignmentSourceType.Gia,
                SourceRowId = row.Id,
                SourceAcademicPlanRecordId = row.AcademicPlanRecordId,
                LoadElementType = LoadAssignmentElementType.GiaWork,
                Title = row.GiaSection,
                Subtitle = $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                ElementDisplayName = row.WorkName,
                TotalHours = RoundHoursToInt(row.TotalHours)
            });
        }

        var groupedItems = rawItems
            .GroupBy(x => new
            {
                x.SourceType,
                x.SourceAcademicPlanRecordId,
                x.LoadElementType
            })
            .Select(g =>
            {
                var first = g.First();

                var totalHours = g.Sum(x => x.TotalHours);

                var assignedHours = assignments
                    .Where(x => x.SourceType == first.SourceType)
                    .Where(x => x.SourceAcademicPlanRecordId == first.SourceAcademicPlanRecordId)
                    .Where(x => x.LoadElementType == first.LoadElementType)
                    .Sum(x => x.AssignedHours);

                return new DistributableLoadItem
                {
                    SourceType = first.SourceType,
                    SourceRowId = first.SourceRowId,
                    SourceAcademicPlanRecordId = first.SourceAcademicPlanRecordId,
                    LoadElementType = first.LoadElementType,
                    Title = first.Title,
                    Subtitle = first.Subtitle,
                    ElementDisplayName = first.ElementDisplayName,
                    TotalHours = totalHours,
                    AssignedHours = assignedHours,
                    RemainingHours = Math.Max(0, totalHours - assignedHours)
                };
            })
            .OrderBy(x => x.SourceType)
            .ThenBy(x => x.Title)
            .ThenBy(x => x.ElementDisplayName)
            .ToList();

        return groupedItems;
    }

    private static void AddItemIfNeeded(List<DistributableLoadItem> result, DistributableLoadItem item)
    {
        if (item.TotalHours <= 0)
        {
            return;
        }

        result.Add(item);
    }

    private async Task<int> GetItemTotalHoursAsync(
        string academicYear,
        LoadAssignmentSourceType sourceType,
        int sourceAcademicPlanRecordId,
        LoadAssignmentElementType elementType)
    {
        var assignments = await _context.LecturerLoadAssignments
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .ToListAsync();

        var items = await BuildDistributableItemsAsync(academicYear, assignments);

        return items
            .Where(x => x.SourceType == sourceType)
            .Where(x => x.SourceAcademicPlanRecordId == sourceAcademicPlanRecordId)
            .Where(x => x.LoadElementType == elementType)
            .Select(x => x.TotalHours)
            .FirstOrDefault();
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

    private static int CalculateLimitHours(int normHours, decimal rate)
    {
        return (int)Math.Round(normHours * rate, MidpointRounding.AwayFromZero);
    }

    private static int RoundHoursToInt(decimal value)
    {
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private static bool IsAssistant(string? studyPostTitle)
    {
        return !string.IsNullOrWhiteSpace(studyPostTitle) &&
               studyPostTitle.Contains("ассист", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildKey(
        LoadAssignmentSourceType sourceType,
        int sourceAcademicPlanRecordId,
        LoadAssignmentElementType elementType)
    {
        return $"{(int)sourceType}_{sourceAcademicPlanRecordId}_{(int)elementType}";
    }

    private static bool TryParseKey(
        string key,
        out LoadAssignmentSourceType sourceType,
        out int sourceAcademicPlanRecordId,
        out LoadAssignmentElementType elementType)
    {
        sourceType = default;
        sourceAcademicPlanRecordId = 0;
        elementType = default;

        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var parts = key.Split('_');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var sourceTypeValue))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out sourceAcademicPlanRecordId))
        {
            return false;
        }

        if (!int.TryParse(parts[2], out var elementTypeValue))
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

        sourceType = (LoadAssignmentSourceType)sourceTypeValue;
        elementType = (LoadAssignmentElementType)elementTypeValue;

        return true;
    }

    private static WorkloadDistributionAvailableItemViewModel MapAvailableItem(DistributableLoadItem item)
    {
        return new WorkloadDistributionAvailableItemViewModel
        {
            ItemKey = BuildKey(item.SourceType, item.SourceAcademicPlanRecordId, item.LoadElementType),
            SourceTypeDisplayName = GetSourceDisplayName(item.SourceType),
            Title = item.Title,
            Subtitle = item.Subtitle,
            ElementDisplayName = item.ElementDisplayName,
            TotalHours = item.TotalHours,
            AssignedHours = item.AssignedHours,
            RemainingHours = item.RemainingHours
        };
    }

    private static string GetLecturerDisplayName(Lecturer? lecturer)
    {
        if (lecturer == null)
        {
            return string.Empty;
        }

        return $"{lecturer.LastName}{lecturer.FirstName}{lecturer.Patronymic}".Trim();
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

    private sealed class DistributableLoadItem
    {
        public LoadAssignmentSourceType SourceType { get; set; }
        public int SourceRowId { get; set; }
        public int SourceAcademicPlanRecordId { get; set; }
        public LoadAssignmentElementType LoadElementType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ElementDisplayName { get; set; } = string.Empty;
        public int TotalHours { get; set; }
        public int AssignedHours { get; set; }
        public int RemainingHours { get; set; }
    }
}

public sealed class WorkloadDistributionOperationResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public int? LecturerId { get; private set; }

    public static WorkloadDistributionOperationResult Ok(string message, int? lecturerId = null)
    {
        return new WorkloadDistributionOperationResult
        {
            Success = true,
            Message = message,
            LecturerId = lecturerId
        };
    }

    public static WorkloadDistributionOperationResult Fail(string message, int? lecturerId = null)
    {
        return new WorkloadDistributionOperationResult
        {
            Success = false,
            Message = message,
            LecturerId = lecturerId
        };
    }
}