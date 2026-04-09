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
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .OrderBy(x => x.Id)
            .ToListAsync();

        var items = await BuildDistributableItemsAsync(academicYear, assignments);

        var itemMap = items.ToDictionary(x => BuildKey(x.SourceType, x.SourceRowId, x.LoadElementType));

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
            AssignedHours = assignments.Sum(x => x.AssignedHours),
            RemainingHours = Math.Max(0, items.Sum(x => x.TotalHours) - assignments.Sum(x => x.AssignedHours)),
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
            var lecturerAssignments = assignments
                .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
                .ToList();

            var assignedHours = lecturerAssignments.Sum(x => x.AssignedHours);
            var limitHours = CalculateLimitHours(plan.LecturerStudyPost?.Hours ?? 0, plan.Rate);
            var isAssistant = IsAssistant(plan.LecturerStudyPost?.StudyPostTitle);

            var existingKeys = lecturerAssignments
                .Select(x => BuildKey(x.SourceType, x.SourceRowId, x.LoadElementType))
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
                .Where(x => !existingKeys.Contains(BuildKey(x.SourceType, x.SourceRowId, x.LoadElementType)))
                .Where(x => !(isAssistant && x.LoadElementType == LoadAssignmentElementType.Lecture))
                .OrderBy(x => x.SourceType)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.ElementDisplayName)
                .Select(MapAvailableItem)
                .ToList();

            foreach (var assignment in lecturerAssignments)
            {
                var key = BuildKey(assignment.SourceType, assignment.SourceRowId, assignment.LoadElementType);
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

        if (!TryParseKey(itemKey, out var sourceType, out var sourceRowId, out var elementType))
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
            x.SourceRowId == sourceRowId &&
            x.LoadElementType == elementType);

        if (alreadyExists)
        {
            return WorkloadDistributionOperationResult.Fail("Этот элемент уже назначен выбранному преподавателю.", lecturerId);
        }

        var assignments = await _context.LecturerLoadAssignments
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .ToListAsync();

        var items = await BuildDistributableItemsAsync(academicYear, assignments);
        var item = items.FirstOrDefault(x =>
            x.SourceType == sourceType &&
            x.SourceRowId == sourceRowId &&
            x.LoadElementType == elementType);

        if (item == null)
        {
            return WorkloadDistributionOperationResult.Fail("Элемент нагрузки не найден.", lecturerId);
        }

        if (item.RemainingHours <= 0)
        {
            return WorkloadDistributionOperationResult.Fail("По этому элементу больше нет свободных часов.", lecturerId);
        }

        var lecturerAssignedHours = assignments
            .Where(x => x.LecturerAcademicYearPlanId == plan.Id)
            .Sum(x => x.AssignedHours);

        var limitHours = CalculateLimitHours(plan.LecturerStudyPost?.Hours ?? 0, plan.Rate);
        var lecturerRemainingHours = limitHours - lecturerAssignedHours;

        if (lecturerRemainingHours <= 0)
        {
            return WorkloadDistributionOperationResult.Fail("У преподавателя уже исчерпан лимит часов.", lecturerId);
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
            SourceRowId = sourceRowId,
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
            assignment.SourceRowId,
            assignment.LoadElementType);

        if (itemTotalHours <= 0)
        {
            _context.LecturerLoadAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return WorkloadDistributionOperationResult.Ok(
                "Исходная нагрузка больше не существует. Назначение удалено.",
                lecturerId);
        }

        var allAssignmentsForYear = await _context.LecturerLoadAssignments
            .Where(x => x.AcademicYear == academicYear)
            .ToListAsync();

        var totalAssignedForItem = allAssignmentsForYear
            .Where(x =>
                x.SourceType == assignment.SourceType &&
                x.SourceRowId == assignment.SourceRowId &&
                x.LoadElementType == assignment.LoadElementType)
            .Sum(x => x.AssignedHours);

        var totalAssignedForLecturer = allAssignmentsForYear
            .Where(x => x.LecturerAcademicYearPlanId == assignment.LecturerAcademicYearPlanId)
            .Sum(x => x.AssignedHours);

        var limitHours = CalculateLimitHours(
            assignment.LecturerAcademicYearPlan.LecturerStudyPost?.Hours ?? 0,
            assignment.LecturerAcademicYearPlan.Rate);

        var maxForItem = itemTotalHours - (totalAssignedForItem - assignment.AssignedHours);
        var maxForLecturer = limitHours - (totalAssignedForLecturer - assignment.AssignedHours);
        var maxAllowed = Math.Max(0, Math.Min(maxForItem, maxForLecturer));

        var newValue = assignment.AssignedHours + delta;
        if (newValue < 0)
        {
            newValue = 0;
        }

        if (newValue > maxAllowed)
        {
            newValue = maxAllowed;
        }

        if (newValue <= 0)
        {
            _context.LecturerLoadAssignments.Remove(assignment);
        }
        else
        {
            assignment.AssignedHours = newValue;
        }

        await _context.SaveChangesAsync();

        return WorkloadDistributionOperationResult.Ok("Часы распределения обновлены.", lecturerId);
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

        var newPlans = missingLecturers.Select(x => new LecturerAcademicYearPlan
        {
            AcademicYear = academicYear,
            LecturerId = x.Id,
            LecturerStudyPostId = x.LecturerStudyPostId,
            Rate = 1.00m
        });

        _context.LecturerAcademicYearPlans.AddRange(newPlans);
        await _context.SaveChangesAsync();
    }

    private async Task<List<DistributableLoadItem>> BuildDistributableItemsAsync(
        string academicYear,
        IReadOnlyCollection<LecturerLoadAssignment> assignments)
    {
        var result = new List<DistributableLoadItem>();

        var workloadRows = await _context.WorkloadRows
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .OrderBy(x => x.Course)
            .ThenBy(x => x.SemesterName)
            .ThenBy(x => x.DisciplineName)
            .ToListAsync();

        foreach (var row in workloadRows)
        {
            AddDisciplineItem(result, row, LoadAssignmentElementType.Lecture, "Лекции", row.LectureTotalHours);
            AddDisciplineItem(result, row, LoadAssignmentElementType.Practice, "Практические занятия", row.PracticeTotalHours);
            AddDisciplineItem(result, row, LoadAssignmentElementType.Laboratory, "Лабораторные занятия", row.LabTotalHours);
            AddDisciplineItem(result, row, LoadAssignmentElementType.Consultation, "Консультации", row.ConsultationHours);
            AddDisciplineItem(result, row, LoadAssignmentElementType.Exam, "Экзамен", row.ExamHours);
            AddDisciplineItem(result, row, LoadAssignmentElementType.Credit, "Зачет", row.CreditHours);
            AddDisciplineItem(result, row, LoadAssignmentElementType.CourseWork, "Курсовая работа", row.CourseWorkHours);
            AddDisciplineItem(result, row, LoadAssignmentElementType.CourseProject, "Курсовой проект", row.CourseProjectHours);
        }

        var practiceRows = await _context.PracticeWorkloadRows
            .AsNoTracking()
            .Where(x => x.PlanYear == academicYear)
            .OrderBy(x => x.Course)
            .ThenBy(x => x.DirectionCode)
            .ThenBy(x => x.PracticeName)
            .ToListAsync();

        foreach (var row in practiceRows)
        {
            AddCommonItem(
                result,
                LoadAssignmentSourceType.Practice,
                row.Id,
                LoadAssignmentElementType.PracticeWork,
                row.PracticeName,
                $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                row.TotalHours);
        }

        var giaRows = await _context.GiaWorkloadRows
            .AsNoTracking()
            .Where(x => x.PlanYear == academicYear)
            .OrderBy(x => x.Course)
            .ThenBy(x => x.DirectionCode)
            .ThenBy(x => x.GiaSection)
            .ThenBy(x => x.WorkName)
            .ToListAsync();

        foreach (var row in giaRows)
        {
            AddCommonItem(
                result,
                LoadAssignmentSourceType.Gia,
                row.Id,
                LoadAssignmentElementType.GiaWork,
                row.WorkName,
                $"{row.GiaSection} · {row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
                row.TotalHours);
        }

        var assignedMap = assignments
            .GroupBy(x => BuildKey(x.SourceType, x.SourceRowId, x.LoadElementType))
            .ToDictionary(x => x.Key, x => x.Sum(y => y.AssignedHours));

        foreach (var item in result)
        {
            var key = BuildKey(item.SourceType, item.SourceRowId, item.LoadElementType);
            item.AssignedHours = assignedMap.TryGetValue(key, out var assignedHours)
                ? assignedHours
                : 0;

            item.RemainingHours = Math.Max(0, item.TotalHours - item.AssignedHours);
        }

        return result;
    }

    private async Task<int> GetItemTotalHoursAsync(
        string academicYear,
        LoadAssignmentSourceType sourceType,
        int sourceRowId,
        LoadAssignmentElementType elementType)
    {
        switch (sourceType)
        {
            case LoadAssignmentSourceType.Discipline:
                {
                    var row = await _context.WorkloadRows
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == sourceRowId && x.AcademicYear == academicYear);

                    if (row == null)
                    {
                        return 0;
                    }

                    return elementType switch
                    {
                        LoadAssignmentElementType.Lecture => RoundHours(row.LectureTotalHours),
                        LoadAssignmentElementType.Practice => RoundHours(row.PracticeTotalHours),
                        LoadAssignmentElementType.Laboratory => RoundHours(row.LabTotalHours),
                        LoadAssignmentElementType.Consultation => RoundHours(row.ConsultationHours),
                        LoadAssignmentElementType.Exam => RoundHours(row.ExamHours),
                        LoadAssignmentElementType.Credit => RoundHours(row.CreditHours),
                        LoadAssignmentElementType.CourseWork => RoundHours(row.CourseWorkHours),
                        LoadAssignmentElementType.CourseProject => RoundHours(row.CourseProjectHours),
                        _ => 0
                    };
                }

            case LoadAssignmentSourceType.Practice:
                {
                    var row = await _context.PracticeWorkloadRows
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == sourceRowId && x.PlanYear == academicYear);

                    return row == null ? 0 : RoundHours(row.TotalHours);
                }

            case LoadAssignmentSourceType.Gia:
                {
                    var row = await _context.GiaWorkloadRows
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == sourceRowId && x.PlanYear == academicYear);

                    return row == null ? 0 : RoundHours(row.TotalHours);
                }

            default:
                return 0;
        }
    }

    private static void AddDisciplineItem(
        List<DistributableLoadItem> items,
        WorkloadRow row,
        LoadAssignmentElementType elementType,
        string elementDisplayName,
        decimal hours)
    {
        AddCommonItem(
            items,
            LoadAssignmentSourceType.Discipline,
            row.Id,
            elementType,
            row.DisciplineName,
            $"{row.DirectionCode} · курс {row.Course} · {row.SemesterName}",
            hours,
            elementDisplayName);
    }

    private static void AddCommonItem(
        List<DistributableLoadItem> items,
        LoadAssignmentSourceType sourceType,
        int sourceRowId,
        LoadAssignmentElementType elementType,
        string title,
        string subtitle,
        decimal hours,
        string? customElementDisplayName = null)
    {
        var roundedHours = RoundHours(hours);
        if (roundedHours <= 0)
        {
            return;
        }

        items.Add(new DistributableLoadItem
        {
            SourceType = sourceType,
            SourceRowId = sourceRowId,
            LoadElementType = elementType,
            Title = title,
            Subtitle = subtitle,
            ElementDisplayName = customElementDisplayName ?? GetElementDisplayName(elementType),
            TotalHours = roundedHours
        });
    }

    private static int RoundHours(decimal hours)
    {
        return (int)Math.Round(hours, 0, MidpointRounding.AwayFromZero);
    }

    private static int CalculateLimitHours(int normHours, decimal rate)
    {
        return (int)Math.Round(normHours * rate, 0, MidpointRounding.AwayFromZero);
    }

    private static string GetLecturerDisplayName(Lecturer? lecturer)
    {
        if (lecturer == null)
        {
            return "Без имени";
        }

        if (!string.IsNullOrWhiteSpace(lecturer.Abbreviation))
        {
            return lecturer.Abbreviation;
        }

        return $"{lecturer.LastName} {lecturer.FirstName} {lecturer.Patronymic}".Trim();
    }

    private static bool IsAssistant(string? studyPostTitle)
    {
        return !string.IsNullOrWhiteSpace(studyPostTitle) &&
               studyPostTitle.Contains("ассист", StringComparison.OrdinalIgnoreCase);
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

    private static string GetElementDisplayName(LoadAssignmentElementType elementType)
    {
        return elementType switch
        {
            LoadAssignmentElementType.Lecture => "Лекции",
            LoadAssignmentElementType.Practice => "Практические занятия",
            LoadAssignmentElementType.Laboratory => "Лабораторные занятия",
            LoadAssignmentElementType.Consultation => "Консультации",
            LoadAssignmentElementType.Exam => "Экзамен",
            LoadAssignmentElementType.Credit => "Зачет",
            LoadAssignmentElementType.CourseWork => "Курсовая работа",
            LoadAssignmentElementType.CourseProject => "Курсовой проект",
            LoadAssignmentElementType.PracticeWork => "Практика",
            LoadAssignmentElementType.GiaWork => "ГИА",
            _ => "Нагрузка"
        };
    }

    private static string BuildKey(
        LoadAssignmentSourceType sourceType,
        int sourceRowId,
        LoadAssignmentElementType elementType)
    {
        return $"{(int)sourceType}|{sourceRowId}|{(int)elementType}";
    }

    private static bool TryParseKey(
        string? key,
        out LoadAssignmentSourceType sourceType,
        out int sourceRowId,
        out LoadAssignmentElementType elementType)
    {
        sourceType = default;
        sourceRowId = 0;
        elementType = default;

        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var parts = key.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var sourceTypeValue) ||
            !int.TryParse(parts[1], out sourceRowId) ||
            !int.TryParse(parts[2], out var elementTypeValue))
        {
            return false;
        }

        if (!Enum.IsDefined(typeof(LoadAssignmentSourceType), sourceTypeValue) ||
            !Enum.IsDefined(typeof(LoadAssignmentElementType), elementTypeValue))
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
            ItemKey = BuildKey(item.SourceType, item.SourceRowId, item.LoadElementType),
            SourceTypeDisplayName = GetSourceDisplayName(item.SourceType),
            Title = item.Title,
            Subtitle = item.Subtitle,
            ElementDisplayName = item.ElementDisplayName,
            TotalHours = item.TotalHours,
            AssignedHours = item.AssignedHours,
            RemainingHours = item.RemainingHours
        };
    }

    private sealed class DistributableLoadItem
    {
        public LoadAssignmentSourceType SourceType { get; set; }

        public int SourceRowId { get; set; }

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
    private WorkloadDistributionOperationResult(bool success, string message, int? lecturerId)
    {
        Success = success;
        Message = message;
        LecturerId = lecturerId;
    }

    public bool Success { get; }

    public string Message { get; }

    public int? LecturerId { get; }

    public static WorkloadDistributionOperationResult Ok(string message, int? lecturerId = null)
    {
        return new WorkloadDistributionOperationResult(true, message, lecturerId);
    }

    public static WorkloadDistributionOperationResult Fail(string message, int? lecturerId = null)
    {
        return new WorkloadDistributionOperationResult(false, message, lecturerId);
    }
}