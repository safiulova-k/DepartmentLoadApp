using ClosedXML.Excel;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.Helpers
{
    public static class ExcelExportHelper
    {
        public static byte[] ExportWorkload(List<WorkloadRow> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Дисциплины");

            var headers = new[]
            {
                "Семестр",
                "Форма обучения",
                "Код направления",
                "Наименование дисциплины",
                "Курс",
                "Студентов",
                "Потоков",
                "Групп",
                "Подгрупп",
                "Лекции (по плану)",
                "Лекции (всего)",
                "Практические занятия (по плану)",
                "Практические занятия (всего)",
                "Лабораторные занятия (по плану)",
                "Лабораторные занятия (всего)",
                "Экзамен",
                "Зачет",
                "Курсовая работа",
                "Курсовой проект",
                "РГР",
                "Экзамен (часы)",
                "Зачет (часы)",
                "Курсовая работа (часы)",
                "Курсовой проект (часы)",
                "РГР (часы)",
                "Консультации",
                "Итого"
            };

            FillHeaders(ws, headers);

            int rowIndex = 2;
            foreach (var row in rows)
            {
                int col = 1;
                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.EducationForm;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.DisciplineName;
                ws.Cell(rowIndex, col++).Value = row.Course;
                ws.Cell(rowIndex, col++).Value = row.StudentsCount;
                ws.Cell(rowIndex, col++).Value = row.FlowCount;
                ws.Cell(rowIndex, col++).Value = row.GroupCount;
                ws.Cell(rowIndex, col++).Value = row.SubgroupCount;
                ws.Cell(rowIndex, col++).Value = row.LecturePlanHours;
                ws.Cell(rowIndex, col++).Value = row.LectureTotalHours;
                ws.Cell(rowIndex, col++).Value = row.PracticePlanHours;
                ws.Cell(rowIndex, col++).Value = row.PracticeTotalHours;
                ws.Cell(rowIndex, col++).Value = row.LabPlanHours;
                ws.Cell(rowIndex, col++).Value = row.LabTotalHours;
                ws.Cell(rowIndex, col++).Value = row.HasExam ? "Да" : "Нет";
                ws.Cell(rowIndex, col++).Value = row.HasCredit ? "Да" : "Нет";
                ws.Cell(rowIndex, col++).Value = row.HasCourseWork ? "Да" : "Нет";
                ws.Cell(rowIndex, col++).Value = row.HasCourseProject ? "Да" : "Нет";
                ws.Cell(rowIndex, col++).Value = row.HasRgr ? "Да" : "Нет";
                ws.Cell(rowIndex, col++).Value = row.ExamHours;
                ws.Cell(rowIndex, col++).Value = row.CreditHours;
                ws.Cell(rowIndex, col++).Value = row.CourseWorkHours;
                ws.Cell(rowIndex, col++).Value = row.CourseProjectHours;
                ws.Cell(rowIndex, col++).Value = row.RgrHours;
                ws.Cell(rowIndex, col++).Value = row.ConsultationHours;
                ws.Cell(rowIndex, col++).Value = row.TotalHours;

                rowIndex++;
            }

            FormatSheet(ws, headers.Length);
            return SaveWorkbook(workbook);
        }

        public static byte[] ExportPractice(IEnumerable<PracticeWorkloadRow> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Практики");

            var headers = new[]
            {
                "Семестр",
                "Форма обучения",
                "Код направления",
                "Вид практики",
                "Курс",
                "Групп",
                "Студентов",
                "Количество недель",
                "Итого"
            };

            FillHeaders(ws, headers);

            var rowIndex = 2;
            foreach (var row in rows)
            {
                var col = 1;
                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.EducationForm;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.PracticeName;
                ws.Cell(rowIndex, col++).Value = row.Course;
                ws.Cell(rowIndex, col++).Value = row.GroupCount;
                ws.Cell(rowIndex, col++).Value = row.StudentsCount;
                ws.Cell(rowIndex, col++).Value = row.WeeksCount;
                ws.Cell(rowIndex, col++).Value = row.TotalHours;
                rowIndex++;
            }

            FormatSheet(ws, headers.Length);
            return SaveWorkbook(workbook);
        }

        public static byte[] ExportGia(IEnumerable<GiaWorkloadRow> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("ГИА");

            var headers = new[]
            {
                "Раздел",
                "Семестр",
                "Форма обучения",
                "Код направления",
                "Вид работы",
                "Курс",
                "Групп",
                "Студентов",
                "Часы вручную",
                "Итого"
            };

            FillHeaders(ws, headers);

            var rowIndex = 2;
            foreach (var row in rows)
            {
                var col = 1;
                ws.Cell(rowIndex, col++).Value = row.GiaSection;
                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.EducationForm;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.WorkName;
                ws.Cell(rowIndex, col++).Value = row.Course;
                ws.Cell(rowIndex, col++).Value = row.GroupCount;
                ws.Cell(rowIndex, col++).Value = row.StudentsCount;
                ws.Cell(rowIndex, col++).Value = row.ManualHours;
                ws.Cell(rowIndex, col++).Value = row.TotalHours;
                rowIndex++;
            }

            FormatSheet(ws, headers.Length);
            return SaveWorkbook(workbook);
        }

        public static byte[] ExportCombinedCalculation(
    string academicYear,
    IEnumerable<WorkloadRow> workloadRows,
    IEnumerable<PracticeWorkloadRow> practiceRows,
    IEnumerable<GiaWorkloadRow> giaRows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Каф. ИС");

            const int lastColumn = 26;

            ws.Range(2, 1, 2, lastColumn).Merge();
            ws.Cell(2, 1).Value = "Расчет учебной нагрузки кафедры";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(3, 1, 3, lastColumn).Merge();
            ws.Cell(3, 1).Value = $"на {academicYear} учебный год";
            ws.Cell(3, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Style.Font.FontSize = 13;
            ws.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(5, 1).Value = "№";
            ws.Cell(5, 2).Value = "Семестр";
            ws.Cell(5, 3).Value = "Форма";
            ws.Cell(5, 4).Value = "Раздел";
            ws.Cell(5, 5).Value = "Код";
            ws.Cell(5, 6).Value = "Наименование";
            ws.Cell(5, 7).Value = "Вид работы";
            ws.Cell(5, 8).Value = "Курс";
            ws.Cell(5, 9).Value = "Студ.";
            ws.Cell(5, 10).Value = "Потоки";
            ws.Cell(5, 11).Value = "Группы";
            ws.Cell(5, 12).Value = "Подгруппы";
            ws.Cell(5, 13).Value = "Недель";
            ws.Cell(5, 14).Value = "Лекции";
            ws.Cell(5, 15).Value = "Практ.";
            ws.Cell(5, 16).Value = "Лаб.";
            ws.Cell(5, 17).Value = "Конс.";
            ws.Cell(5, 18).Value = "Экз.";
            ws.Cell(5, 19).Value = "Зач.";
            ws.Cell(5, 20).Value = "Курс. раб.";
            ws.Cell(5, 21).Value = "Курс. проект";
            ws.Cell(5, 22).Value = "РГР";
            ws.Cell(5, 23).Value = "Практика";
            ws.Cell(5, 24).Value = "ГИА";
            ws.Cell(5, 25).Value = "Ручные";
            ws.Cell(5, 26).Value = "Итого";

            var headerRange = ws.Range(5, 1, 5, lastColumn);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.WrapText = true;

            var rowIndex = 6;
            var number = 1;

            rowIndex = FillSectionHeader(ws, rowIndex, lastColumn, "ДИСЦИПЛИНЫ");

            foreach (var row in workloadRows)
            {
                var col = 1;
                ws.Cell(rowIndex, col++).Value = number++;
                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.EducationForm;
                ws.Cell(rowIndex, col++).Value = "Дисциплина";
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.DisciplineName;
                ws.Cell(rowIndex, col++).Value = "Учебная работа";
                ws.Cell(rowIndex, col++).Value = row.Course;
                ws.Cell(rowIndex, col++).Value = row.StudentsCount;
                ws.Cell(rowIndex, col++).Value = row.FlowCount;
                ws.Cell(rowIndex, col++).Value = row.GroupCount;
                ws.Cell(rowIndex, col++).Value = row.SubgroupCount;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = row.LectureTotalHours;
                ws.Cell(rowIndex, col++).Value = row.PracticeTotalHours;
                ws.Cell(rowIndex, col++).Value = row.LabTotalHours;
                ws.Cell(rowIndex, col++).Value = row.ConsultationHours;
                ws.Cell(rowIndex, col++).Value = row.ExamHours;
                ws.Cell(rowIndex, col++).Value = row.CreditHours;
                ws.Cell(rowIndex, col++).Value = row.CourseWorkHours;
                ws.Cell(rowIndex, col++).Value = row.CourseProjectHours;
                ws.Cell(rowIndex, col++).Value = row.RgrHours;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = row.TotalHours;

                rowIndex++;
            }

            rowIndex = FillSectionHeader(ws, rowIndex, lastColumn, "ПРАКТИКИ");

            foreach (var row in practiceRows)
            {
                var col = 1;
                ws.Cell(rowIndex, col++).Value = number++;
                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.EducationForm;
                ws.Cell(rowIndex, col++).Value = "Практика";
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.PracticeName;
                ws.Cell(rowIndex, col++).Value = "Практика";
                ws.Cell(rowIndex, col++).Value = row.Course;
                ws.Cell(rowIndex, col++).Value = row.StudentsCount;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = row.GroupCount;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = row.WeeksCount;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = row.TotalHours;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = row.TotalHours;

                rowIndex++;
            }

            rowIndex = FillSectionHeader(ws, rowIndex, lastColumn, "ГИА");

            foreach (var row in giaRows)
            {
                var col = 1;
                ws.Cell(rowIndex, col++).Value = number++;
                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.EducationForm;
                ws.Cell(rowIndex, col++).Value = "ГИА";
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.GiaSection;
                ws.Cell(rowIndex, col++).Value = row.WorkName;
                ws.Cell(rowIndex, col++).Value = row.Course;
                ws.Cell(rowIndex, col++).Value = row.StudentsCount;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = row.GroupCount;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = 0;
                ws.Cell(rowIndex, col++).Value = row.TotalHours;
                ws.Cell(rowIndex, col++).Value = row.ManualHours;
                ws.Cell(rowIndex, col++).Value = row.TotalHours;

                rowIndex++;
            }

            var usedRange = ws.Range(5, 1, rowIndex - 1, lastColumn);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            usedRange.Style.Alignment.WrapText = true;

            ws.Columns().AdjustToContents();

            ws.Column(1).Width = 6;
            ws.Column(2).Width = 10;
            ws.Column(3).Width = 10;
            ws.Column(4).Width = 14;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 38;
            ws.Column(7).Width = 28;
            ws.Column(8).Width = 8;
            ws.Column(9).Width = 10;
            ws.Column(10).Width = 10;
            ws.Column(11).Width = 10;
            ws.Column(12).Width = 12;
            ws.Column(13).Width = 10;
            ws.Column(14).Width = 10;
            ws.Column(15).Width = 10;
            ws.Column(16).Width = 10;
            ws.Column(17).Width = 10;
            ws.Column(18).Width = 10;
            ws.Column(19).Width = 10;
            ws.Column(20).Width = 12;
            ws.Column(21).Width = 14;
            ws.Column(22).Width = 10;
            ws.Column(23).Width = 12;
            ws.Column(24).Width = 12;
            ws.Column(25).Width = 10;
            ws.Column(26).Width = 12;

            ws.SheetView.FreezeRows(5);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static int FillSectionTitle(IXLWorksheet ws, int rowIndex, int lastColumn, string title)
        {
            ws.Range(rowIndex, 1, rowIndex, lastColumn).Merge();
            ws.Cell(rowIndex, 1).Value = title;
            ws.Cell(rowIndex, 1).Style.Font.Bold = true;
            ws.Cell(rowIndex, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
            ws.Cell(rowIndex, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(rowIndex, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            return rowIndex + 1;
        }

        private static int FillTotalRow(IXLWorksheet ws, int rowIndex, int lastColumn, string title)
        {
            ws.Range(rowIndex, 1, rowIndex, lastColumn - 1).Merge();
            ws.Cell(rowIndex, 1).Value = title;
            ws.Cell(rowIndex, 1).Style.Font.Bold = true;
            ws.Cell(rowIndex, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#EEECE1");

            ws.Cell(rowIndex, lastColumn).FormulaA1 = $"SUM(X{GetSectionStartRow(ws, rowIndex)}:X{rowIndex - 1})";
            ws.Cell(rowIndex, lastColumn).Style.Font.Bold = true;
            ws.Cell(rowIndex, lastColumn).Style.Fill.BackgroundColor = XLColor.FromHtml("#EEECE1");

            return rowIndex + 1;
        }

        private static int GetSectionStartRow(IXLWorksheet ws, int totalRow)
        {
            for (int row = totalRow - 1; row >= 1; row--)
            {
                if (ws.Cell(row, 1).IsMerged() &&
                    ws.Cell(row, 1).GetString() is "ДИСЦИПЛИНЫ" or "ПРАКТИКИ" or "ГИА")
                {
                    return row + 1;
                }
            }

            return 5;
        }

        private static void FillHeaders(IXLWorksheet ws, string[] headers, int headerRow = 1)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
                ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
                ws.Cell(headerRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(headerRow, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(headerRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(headerRow, i + 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private static void FormatSheet(IXLWorksheet ws, int lastColumn)
        {
            var headerRange = ws.Range(1, 1, 1, lastColumn);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.SheetView.FreezeRows(1);
            ws.Columns(1, lastColumn).AdjustToContents();

            var usedRange = ws.RangeUsed();
            if (usedRange != null)
            {
                usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Alignment.WrapText = true;
            }
        }

        private static void ApplyBorders(IXLWorksheet ws, int fromRow, int toRow, int lastColumn)
        {
            var range = ws.Range(fromRow, 1, toRow, lastColumn);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Alignment.WrapText = true;
        }

        private static void ApplyNumberFormat(IXLWorksheet ws, int fromRow, int toRow, int fromColumn, int toColumn)
        {
            for (int row = fromRow; row <= toRow; row++)
            {
                for (int col = fromColumn; col <= toColumn; col++)
                {
                    ws.Cell(row, col).Style.NumberFormat.Format = "0.##";
                }
            }
        }

        private static byte[] SaveWorkbook(XLWorkbook workbook)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static int FillSectionHeader(IXLWorksheet ws, int rowIndex, int lastColumn, string title)
        {
            ws.Range(rowIndex, 1, rowIndex, lastColumn).Merge();
            ws.Cell(rowIndex, 1).Value = title;
            ws.Cell(rowIndex, 1).Style.Font.Bold = true;
            ws.Cell(rowIndex, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(rowIndex, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#DDEBF7");
            ws.Cell(rowIndex, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            return rowIndex + 1;
        }
    }
}