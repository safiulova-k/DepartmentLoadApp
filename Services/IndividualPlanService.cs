using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Core;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.ViewModels.IndividualPlans;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Services;

public class IndividualPlanService
{
    private const string TemplateFolderName = "Templates";
    private const string TemplateFileName = "IndividualPlanTemplate.xlsx";

    private readonly DepartmentLoadDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public IndividualPlanService(
        DepartmentLoadDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IndividualPlansPageViewModel> BuildPageAsync(int? startYear)
    {
        var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
        var academicYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

        await EnsureAcademicYearPlansAsync(academicYear);

        var plans = await _context.LecturerAcademicYearPlans
            .AsNoTracking()
            .Include(x => x.Lecturer)
            .Include(x => x.LecturerStudyPost)
            .Where(x => x.AcademicYear == academicYear)
            .OrderBy(x => x.Lecturer!.LastName)
            .ThenBy(x => x.Lecturer!.FirstName)
            .ThenBy(x => x.Lecturer!.Patronymic)
            .ToListAsync();

        var assignedHoursMap = await _context.LecturerLoadAssignments
            .AsNoTracking()
            .Where(x => x.AcademicYear == academicYear)
            .GroupBy(x => x.LecturerAcademicYearPlanId)
            .Select(g => new
            {
                LecturerAcademicYearPlanId = g.Key,
                AssignedHours = g.Sum(x => x.AssignedHours)
            })
            .ToDictionaryAsync(x => x.LecturerAcademicYearPlanId, x => x.AssignedHours);

        var model = new IndividualPlansPageViewModel
        {
            SelectedYearStart = selectedYearStart,
            SelectedAcademicYear = academicYear,
            AvailableYearStarts = AcademicYearResolver.BuildAvailableStartYears(selectedYearStart),
            TemplateExists = File.Exists(GetTemplatePath())
        };

        model.Lecturers = plans
            .Select(plan => new IndividualPlanLecturerRowViewModel
            {
                LecturerId = plan.LecturerId,
                FullName = BuildLecturerFullName(plan.Lecturer),
                StudyPostTitle = plan.LecturerStudyPost?.StudyPostTitle ?? "Не указана",
                Rate = plan.Rate,
                AssignedHours = assignedHoursMap.TryGetValue(plan.Id, out var hours) ? hours : 0
            })
            .ToList();

        return model;
    }

    public async Task<IndividualPlanExportResult> ExportLecturerPlanAsync(int startYear, int lecturerId)
    {
        var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
        var academicYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

        await EnsureAcademicYearPlansAsync(academicYear);

        var templatePath = GetTemplatePath();
        if (!File.Exists(templatePath))
        {
            return IndividualPlanExportResult.Fail(
                $"Не найден шаблон Excel: {templatePath}");
        }

        var plan = await _context.LecturerAcademicYearPlans
            .Include(x => x.Lecturer)
            .Include(x => x.LecturerStudyPost)
            .FirstOrDefaultAsync(x =>
                x.AcademicYear == academicYear &&
                x.LecturerId == lecturerId);

        if (plan == null || plan.Lecturer == null)
        {
            return IndividualPlanExportResult.Fail("План преподавателя не найден.");
        }

        var planRows = await BuildPlanRowsAsync(academicYear, plan.Id);

        using var workbook = new XLWorkbook(templatePath);

        FillTitleSheet(workbook, plan, academicYear);
        FillSummarySheet(workbook, plan, academicYear);

        FillAutumnSheet(
            workbook,
            academicYear,
            planRows.Where(x => x.Semester == SemesterKind.Autumn).ToList());

        FillSpringSheet(
            workbook,
            academicYear,
            planRows.Where(x => x.Semester == SemesterKind.Spring).ToList());

        workbook.CalculateMode = XLCalculateMode.Auto;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var lecturerNamePart = BuildSafeFileNamePart(
            $"{plan.Lecturer.LastName}_{plan.Lecturer.FirstName}_{plan.Lecturer.Patronymic}");

        return IndividualPlanExportResult.Ok(
            content: stream.ToArray(),
            fileName: $"Индивидуальный_план_{lecturerNamePart}_{academicYear}.xlsx",
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    public async Task<IndividualPlanExportResult> ExportAllPlansAsync(int startYear)
    {
        var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
        var academicYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

        await EnsureAcademicYearPlansAsync(academicYear);

        var plans = await _context.LecturerAcademicYearPlans
            .AsNoTracking()
            .Include(x => x.Lecturer)
            .Include(x => x.LecturerStudyPost)
            .Where(x => x.AcademicYear == academicYear)
            .OrderBy(x => x.Lecturer!.LastName)
            .ThenBy(x => x.Lecturer!.FirstName)
            .ThenBy(x => x.Lecturer!.Patronymic)
            .ToListAsync();

        if (plans.Count == 0)
        {
            return IndividualPlanExportResult.Fail("Для выбранного учебного года преподаватели не найдены.");
        }

        using var zipStream = new MemoryStream();

        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var plan in plans)
            {
                var fileResult = await ExportLecturerPlanAsync(selectedYearStart, plan.LecturerId);

                if (!fileResult.Success || fileResult.Content == null)
                {
                    continue;
                }

                var entry = archive.CreateEntry(fileResult.FileName, CompressionLevel.Fastest);

                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(fileResult.Content, 0, fileResult.Content.Length);
            }
        }

        return IndividualPlanExportResult.Ok(
            content: zipStream.ToArray(),
            fileName: $"Индивидуальные_планы_{academicYear}.zip",
            contentType: "application/zip");
    }

    private async Task<List<IndividualPlanRowData>> BuildPlanRowsAsync(string academicYear, int lecturerAcademicYearPlanId)
    {
        var assignments = await _context.LecturerLoadAssignments
            .AsNoTracking()
            .Where(x =>
                x.AcademicYear == academicYear &&
                x.LecturerAcademicYearPlanId == lecturerAcademicYearPlanId)
            .OrderBy(x => x.SourceType)
            .ThenBy(x => x.SourceAcademicPlanRecordId)
            .ThenBy(x => x.LoadElementType)
            .ToListAsync();

        if (assignments.Count == 0)
        {
            return new List<IndividualPlanRowData>();
        }

        var disciplinePlanRecordIds = assignments
            .Where(x => x.SourceType == LoadAssignmentSourceType.Discipline)
            .Select(x => x.SourceAcademicPlanRecordId)
            .Distinct()
            .ToList();

        var practicePlanRecordIds = assignments
            .Where(x => x.SourceType == LoadAssignmentSourceType.Practice)
            .Select(x => x.SourceAcademicPlanRecordId)
            .Distinct()
            .ToList();

        var giaPlanRecordIds = assignments
            .Where(x => x.SourceType == LoadAssignmentSourceType.Gia)
            .Select(x => x.SourceAcademicPlanRecordId)
            .Distinct()
            .ToList();

        var disciplineRows = await _context.WorkloadRows
             .AsNoTracking()
             .Where(x => x.AcademicYear == academicYear && disciplinePlanRecordIds.Contains(x.AcademicPlanRecordId))
             .ToListAsync();

        var disciplineMap = disciplineRows
            .GroupBy(x => x.AcademicPlanRecordId)
            .ToDictionary(x => x.Key, x => x.First());

        var practiceRows = await _context.PracticeWorkloadRows
            .AsNoTracking()
            .Where(x => x.PlanYear == academicYear && practicePlanRecordIds.Contains(x.AcademicPlanRecordId))
            .ToListAsync();

        var practiceMap = practiceRows
            .GroupBy(x => x.AcademicPlanRecordId)
            .ToDictionary(x => x.Key, x => x.First());

        var giaRows = await _context.GiaWorkloadRows
            .AsNoTracking()
            .Where(x => x.PlanYear == academicYear && giaPlanRecordIds.Contains(x.AcademicPlanRecordId))
            .ToListAsync();

        var giaMap = giaRows
            .GroupBy(x => x.AcademicPlanRecordId)
            .ToDictionary(x => x.Key, x => x.First());

        var result = new Dictionary<string, IndividualPlanRowData>();

        foreach (var assignment in assignments)
        {
            switch (assignment.SourceType)
            {
                case LoadAssignmentSourceType.Discipline:
                    {
                        if (!disciplineMap.TryGetValue(assignment.SourceAcademicPlanRecordId, out var row))
                        {
                            continue;
                        }

                        var semester = ResolveSemester(row.SemesterName);
                        var key = BuildRowKey(assignment.SourceType, assignment.SourceAcademicPlanRecordId, semester);

                        if (!result.TryGetValue(key, out var item))
                        {
                            item = new IndividualPlanRowData
                            {
                                Key = key,
                                Semester = semester,
                                SortOrder = 1,
                                DisplayText = $"{row.DirectionCode} {row.DisciplineName}",
                                StudentsCount = row.StudentsCount
                            };

                            result[key] = item;
                        }

                        AddHoursToRow(item, assignment.LoadElementType, assignment.AssignedHours);
                        break;
                    }

                case LoadAssignmentSourceType.Practice:
                    {
                        if (!practiceMap.TryGetValue(assignment.SourceAcademicPlanRecordId, out var row))
                        {
                            continue;
                        }

                        var semester = ResolveSemester(row.SemesterName);
                        var key = BuildRowKey(assignment.SourceType, assignment.SourceAcademicPlanRecordId, semester);

                        if (!result.TryGetValue(key, out var item))
                        {
                            item = new IndividualPlanRowData
                            {
                                Key = key,
                                Semester = semester,
                                SortOrder = 2,
                                DisplayText = $"{row.DirectionCode} {row.PracticeName}",
                                StudentsCount = row.StudentsCount
                            };

                            result[key] = item;
                        }

                        AddHoursToRow(item, assignment.LoadElementType, assignment.AssignedHours);
                        break;
                    }

                case LoadAssignmentSourceType.Gia:
                    {
                        if (!giaMap.TryGetValue(assignment.SourceAcademicPlanRecordId, out var row))
                        {
                            continue;
                        }

                        var semester = ResolveSemester(row.SemesterName);
                        var key = BuildRowKey(assignment.SourceType, assignment.SourceAcademicPlanRecordId, semester);

                        if (!result.TryGetValue(key, out var item))
                        {
                            item = new IndividualPlanRowData
                            {
                                Key = key,
                                Semester = semester,
                                SortOrder = 3,
                                DisplayText = $"{row.DirectionCode} {row.GiaSection}: {row.WorkName}",
                                StudentsCount = row.StudentsCount
                            };

                            result[key] = item;
                        }

                        AddHoursToRow(item, assignment.LoadElementType, assignment.AssignedHours);
                        break;
                    }
            }
        }

        return result.Values
            .OrderBy(x => x.Semester)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.DisplayText)
            .ToList();
    }

    private static void FillTitleSheet(
     XLWorkbook workbook,
     LecturerAcademicYearPlan plan,
     string academicYear)
    {
        var sheet = workbook.Worksheet("Титул");
        var lecturer = plan.Lecturer!;

        sheet.Cell("F17").Value = lecturer.FirstName;
        sheet.Cell("F18").Value = lecturer.LastName;
        sheet.Cell("F19").Value = lecturer.Patronymic;
        sheet.Cell("F20").Value = plan.LecturerStudyPost?.StudyPostTitle ?? string.Empty;
        sheet.Cell("F21").Value = lecturer.DateBirth.Year;

        sheet.Cell("F22").Value = string.Empty;
        sheet.Cell("F23").Value = string.Empty;
    }
    private static void FillSummarySheet(
    XLWorkbook workbook,
    LecturerAcademicYearPlan plan,
    string academicYear)
    {
        var sheet = workbook.Worksheet("Сводная таблица");
        var academicYearForTemplate = academicYear.Replace("-", "/");

        sheet.Cell("A3").Value = $"на {academicYearForTemplate} учебный год";
        sheet.Cell("H4").Value = plan.Rate;
        sheet.Cell("H4").Style.NumberFormat.Format = "0.##";
    }
    private static void FillAutumnSheet(
     XLWorkbook workbook,
     string academicYear,
     List<IndividualPlanRowData> rows)
    {
        var sheet = workbook.Worksheet("Осенний сем.");
        var startYear = academicYear.Split('-')[0];
        var academicYearForTemplate = academicYear.Replace("-", "/");

        sheet.Cell("D2").Value =
            $"1.1 Нагрузка преподавателя по программам высшего образования (ВО)     {academicYearForTemplate}уч. год ";

        sheet.Cell("I3").Value = "a) Осенний семестр";
        sheet.Cell("A4").Value = $"\"____\"______________________{startYear}г";

        FillSemesterSheet(
            sheet,
            rows,
            dataStartRow: 8,
            dataEndRow: 19,
            totalRow: 20,
            actualRow: 21,
            yearTotalRow: null,
            yearActualRow: null);
    }

    private static void FillSpringSheet(
        XLWorkbook workbook,
        string academicYear,
        List<IndividualPlanRowData> rows)
    {
        var sheet = workbook.Worksheet("Весенний сем.");

        sheet.Cell("A1").Value = "a) Весенний семестр";

        FillSemesterSheet(
            sheet,
            rows,
            dataStartRow: 5,
            dataEndRow: 17,
            totalRow: 18,
            actualRow: 19,
            yearTotalRow: 20,
            yearActualRow: 21);
    }

    private static void FillSemesterSheet(
     IXLWorksheet sheet,
     List<IndividualPlanRowData> rows,
     int dataStartRow,
     int dataEndRow,
     int totalRow,
     int actualRow,
     int? yearTotalRow,
     int? yearActualRow)
    {
        const int lastColumn = 22;
        var templateCapacity = dataEndRow - dataStartRow + 1;
        var extraRows = Math.Max(0, rows.Count - templateCapacity);

        if (extraRows > 0)
        {
            sheet.Row(totalRow).InsertRowsAbove(extraRows);

            for (var i = 0; i < extraRows; i++)
            {
                var newRow = totalRow + i;
                var templateRow = totalRow - 1;

                for (var col = 1; col <= lastColumn; col++)
                {
                    sheet.Cell(newRow, col).Style = sheet.Cell(templateRow, col).Style;
                    sheet.Cell(newRow, col).Clear(XLClearOptions.Contents);
                }

                sheet.Range(newRow, 1, newRow, 3).Unmerge();
                sheet.Range(newRow, 1, newRow, 3).Merge();
            }

            dataEndRow += extraRows;
            totalRow += extraRows;
            actualRow += extraRows;

            if (yearTotalRow.HasValue)
            {
                yearTotalRow += extraRows;
            }

            if (yearActualRow.HasValue)
            {
                yearActualRow += extraRows;
            }
        }

        for (var row = dataStartRow; row <= dataEndRow; row++)
        {
            for (var col = 1; col <= lastColumn; col++)
            {
                sheet.Cell(row, col).Clear(XLClearOptions.Contents);
            }

            sheet.Range(row, 1, row, 3).Unmerge();
            sheet.Range(row, 1, row, 3).Merge();
        }

        for (var i = 0; i < rows.Count; i++)
        {
            var targetRow = dataStartRow + i;
            var item = rows[i];

            sheet.Cell(targetRow, 1).Value = item.DisplayText;
            sheet.Cell(targetRow, 4).Value = item.StudentsCount == 0 ? string.Empty : item.StudentsCount;

            SetHourValue(sheet, targetRow, 5, item.LectureHours);              // E Лекции
            SetHourValue(sheet, targetRow, 6, item.PracticeHours);             // F Практ.
            SetHourValue(sheet, targetRow, 7, item.LaboratoryHours);           // G Лаб.
            SetHourValue(sheet, targetRow, 8, item.CourseProjectHours);        // H Курсовое проектирование
            SetHourValue(sheet, targetRow, 9, item.ConsultationHours);         // I Консультации
            SetHourValue(sheet, targetRow, 10, item.CreditHours);              // J Зачеты
            SetHourValue(sheet, targetRow, 11, item.ExamHours);                // K Экзамены
            SetHourValue(sheet, targetRow, 13, item.PracticeHoursGuidance);    // M Руководство практиками
            SetHourValue(sheet, targetRow, 17, item.GiaHours);                 // Q Работа в ГИА
            SetHourValue(sheet, targetRow, 20, item.OtherHours);               // T Другие виды работ

            sheet.Cell(targetRow, 21).FormulaA1 = $"SUM(E{targetRow}:T{targetRow})";
            sheet.Cell(targetRow, 22).FormulaA1 = $"U{targetRow}";
        }

        sheet.Cell(totalRow, 1).Value = "Итого за семестр";

        for (var col = 5; col <= 21; col++)
        {
            var letter = XLHelper.GetColumnLetterFromNumber(col);
            sheet.Cell(totalRow, col).FormulaA1 = $"SUM({letter}{dataStartRow}:{letter}{dataEndRow})";
        }

        sheet.Cell(totalRow, 22).FormulaA1 = $"U{totalRow}";

        sheet.Cell(actualRow, 1).Value = "Фактически выполнено";

        for (var col = 5; col <= 22; col++)
        {
            var letter = XLHelper.GetColumnLetterFromNumber(col);
            sheet.Cell(actualRow, col).FormulaA1 = $"{letter}{totalRow}";
        }

        if (yearTotalRow.HasValue)
        {
            sheet.Cell(yearTotalRow.Value, 1).Value = "Итого за учебный год по ВО";

            for (var col = 5; col <= 22; col++)
            {
                var letter = XLHelper.GetColumnLetterFromNumber(col);
                sheet.Cell(yearTotalRow.Value, col).FormulaA1 =
                    $"SUM({letter}{totalRow},'Осенний сем.'!{letter}{(totalRow == 18 ? 20 : 20)})";
            }
        }

        if (yearActualRow.HasValue)
        {
            sheet.Cell(yearActualRow.Value, 1).Value = "Фактически  выполнено за учебный год по ВО";

            for (var col = 5; col <= 22; col++)
            {
                var letter = XLHelper.GetColumnLetterFromNumber(col);
                sheet.Cell(yearActualRow.Value, col).FormulaA1 =
                    $"SUM({letter}{actualRow},'Осенний сем.'!{letter}21)";
            }
        }
    }

    private static void SetHourValue(IXLWorksheet sheet, int row, int column, int value)
    {
        sheet.Cell(row, column).Value = value <= 0 ? string.Empty : value;
    }

    private static void AddHoursToRow(
        IndividualPlanRowData row,
        LoadAssignmentElementType elementType,
        int hours)
    {
        switch (elementType)
        {
            case LoadAssignmentElementType.Lecture:
                row.LectureHours += hours;
                break;

            case LoadAssignmentElementType.Practice:
                row.PracticeHours += hours;
                break;

            case LoadAssignmentElementType.Laboratory:
                row.LaboratoryHours += hours;
                break;

            case LoadAssignmentElementType.CourseProject:
                row.CourseProjectHours += hours;
                break;

            case LoadAssignmentElementType.Consultation:
                row.ConsultationHours += hours;
                break;

            case LoadAssignmentElementType.Credit:
                row.CreditHours += hours;
                break;

            case LoadAssignmentElementType.Exam:
                row.ExamHours += hours;
                break;

            case LoadAssignmentElementType.PracticeWork:
                row.PracticeHoursGuidance += hours;
                break;

            case LoadAssignmentElementType.GiaWork:
                row.GiaHours += hours;
                break;

            case LoadAssignmentElementType.CourseWork:
                row.OtherHours += hours;
                break;
        }
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

    private string GetTemplatePath()
    {
        return Path.Combine(
            _environment.ContentRootPath,
            TemplateFolderName,
            TemplateFileName);
    }

    private static SemesterKind ResolveSemester(string? semesterName)
    {
        if (string.IsNullOrWhiteSpace(semesterName))
        {
            return SemesterKind.Spring;
        }

        var value = semesterName.Trim().ToLowerInvariant();

        if (value.Contains("осен"))
        {
            return SemesterKind.Autumn;
        }

        if (value.Contains("весен"))
        {
            return SemesterKind.Spring;
        }

        var match = Regex.Match(value, @"\d+");
        if (match.Success && int.TryParse(match.Value, out var semesterNumber))
        {
            return semesterNumber % 2 == 1
                ? SemesterKind.Autumn
                : SemesterKind.Spring;
        }

        return SemesterKind.Spring;
    }

    private static string BuildRowKey(
        LoadAssignmentSourceType sourceType,
        int sourceAcademicPlanRecordId,
        SemesterKind semester)
    {
        return $"{sourceType}_{sourceAcademicPlanRecordId}_{semester}";
    }

    private static string BuildLecturerFullName(Lecturer? lecturer)
    {
        if (lecturer == null)
        {
            return string.Empty;
        }

        return $"{lecturer.LastName} {lecturer.FirstName} {lecturer.Patronymic}".Trim();
    }

    private static string BuildSafeFileNamePart(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray());

        while (sanitized.Contains("__", StringComparison.Ordinal))
        {
            sanitized = sanitized.Replace("__", "_", StringComparison.Ordinal);
        }

        return sanitized.Trim('_');
    }

    private sealed class IndividualPlanRowData
    {
        public string Key { get; set; } = string.Empty;
        public SemesterKind Semester { get; set; }
        public int SortOrder { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public int StudentsCount { get; set; }

        public int LectureHours { get; set; }
        public int PracticeHours { get; set; }
        public int LaboratoryHours { get; set; }
        public int CourseProjectHours { get; set; }
        public int ConsultationHours { get; set; }
        public int CreditHours { get; set; }
        public int ExamHours { get; set; }
        public int PracticeHoursGuidance { get; set; }
        public int GiaHours { get; set; }
        public int OtherHours { get; set; }
    }

    private enum SemesterKind
    {
        Autumn = 1,
        Spring = 2
    }
}

public sealed class IndividualPlanExportResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public byte[]? Content { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;

    public static IndividualPlanExportResult Ok(
        byte[] content,
        string fileName,
        string contentType)
    {
        return new IndividualPlanExportResult
        {
            Success = true,
            Content = content,
            FileName = fileName,
            ContentType = contentType
        };
    }

    public static IndividualPlanExportResult Fail(string message)
    {
        return new IndividualPlanExportResult
        {
            Success = false,
            Message = message
        };
    }
}