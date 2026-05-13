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
                "Код направления",
                "Наименование дисциплины",
                "Курс",
                "Студентов",
                "Потоков",
                "Групп",
                "Подгрупп",
                "Лекции",
                "Практические занятия",
                "Лабораторные занятия",
                "Консультации",
                "Экзамен",
                "Зачет",
                "Курсовая работа",
                "Курсовой проект",
                "РГР",
                "Итого"
            };

            FillVerticalHeaders(ws, headers);

            var rowIndex = 2;

            foreach (var row in rows)
            {
                var col = 1;

                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.DisciplineName;
                SetNumber(ws.Cell(rowIndex, col++), row.Course);
                SetNumber(ws.Cell(rowIndex, col++), row.StudentsCount);
                SetNumber(ws.Cell(rowIndex, col++), row.FlowCount);
                SetNumber(ws.Cell(rowIndex, col++), row.GroupCount);
                SetNumber(ws.Cell(rowIndex, col++), row.SubgroupCount);
                SetNumber(ws.Cell(rowIndex, col++), row.LectureTotalHours);
                SetNumber(ws.Cell(rowIndex, col++), row.PracticeTotalHours);
                SetNumber(ws.Cell(rowIndex, col++), row.LabTotalHours);
                SetNumber(ws.Cell(rowIndex, col++), row.ConsultationHours);
                SetNumber(ws.Cell(rowIndex, col++), row.ExamHours);
                SetNumber(ws.Cell(rowIndex, col++), row.CreditHours);
                SetNumber(ws.Cell(rowIndex, col++), row.CourseWorkHours);
                SetNumber(ws.Cell(rowIndex, col++), row.CourseProjectHours);
                SetNumber(ws.Cell(rowIndex, col++), row.RgrHours);
                SetNumber(ws.Cell(rowIndex, col++), row.TotalHours);

                rowIndex++;
            }

            FormatSimpleSheet(ws, headers.Length, rowIndex - 1);

            return SaveWorkbook(workbook);
        }

        public static byte[] ExportPractice(IEnumerable<PracticeWorkloadRow> rows)
        {
            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Практики");

            var headers = new[]
            {
                "Семестр",
                "Код направления",
                "Вид практики",
                "Курс",
                "Групп",
                "Студентов",
                "Практика",
                "Итого"
            };

            FillVerticalHeaders(ws, headers);

            var rowIndex = 2;

            foreach (var row in rows)
            {
                var col = 1;

                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.PracticeName;
                SetNumber(ws.Cell(rowIndex, col++), row.Course);
                SetNumber(ws.Cell(rowIndex, col++), row.GroupCount);
                SetNumber(ws.Cell(rowIndex, col++), row.StudentsCount);
                SetNumber(ws.Cell(rowIndex, col++), row.WeeksCount);
                SetNumber(ws.Cell(rowIndex, col++), row.TotalHours);

                rowIndex++;
            }

            FormatSimpleSheet(ws, headers.Length, rowIndex - 1);

            return SaveWorkbook(workbook);
        }

        public static byte[] ExportGia(IEnumerable<GiaWorkloadRow> rows)
        {
            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("ГИА");

            var headers = new[]
            {
                "Семестр",
                "Код направления",
                "Наименование",
                "Курс",
                "Групп",
                "Студентов",
                "Итого"
            };

            FillVerticalHeaders(ws, headers);

            var rowIndex = 2;

            foreach (var row in rows)
            {
                var col = 1;

                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = $"{row.GiaSection}: {row.WorkName}";
                SetNumber(ws.Cell(rowIndex, col++), row.Course);
                SetNumber(ws.Cell(rowIndex, col++), row.GroupCount);
                SetNumber(ws.Cell(rowIndex, col++), row.StudentsCount);
                SetNumber(ws.Cell(rowIndex, col++), row.TotalHours);

                rowIndex++;
            }

            FormatSimpleSheet(ws, headers.Length, rowIndex - 1);

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

            const int lastColumn = 20;

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

            var headers = new[]
            {
                "Семестр",
                "Код направления",
                "Наименование",
                "Курс",
                "Студентов",
                "Потоки",
                "Группы",
                "Подгруппы",
                "Лекции",
                "Практические занятия",
                "Лабораторные занятия",
                "Консультации",
                "Экзамен",
                "Зачет",
                "Курсовая работа",
                "Курсовой проект",
                "РГР",
                "Практика",
                "ГИА",
                "Итого"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(5, i + 1);

                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.TextRotation = 90;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Row(5).Height = 95;

            var rowIndex = 6;

            rowIndex = FillSectionHeader(ws, rowIndex, lastColumn, "ДИСЦИПЛИНЫ");

            foreach (var row in workloadRows)
            {
                var col = 1;

                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.DisciplineName;
                SetNumber(ws.Cell(rowIndex, col++), row.Course);
                SetNumber(ws.Cell(rowIndex, col++), row.StudentsCount);
                SetNumber(ws.Cell(rowIndex, col++), row.FlowCount);
                SetNumber(ws.Cell(rowIndex, col++), row.GroupCount);
                SetNumber(ws.Cell(rowIndex, col++), row.SubgroupCount);
                SetNumber(ws.Cell(rowIndex, col++), row.LectureTotalHours);
                SetNumber(ws.Cell(rowIndex, col++), row.PracticeTotalHours);
                SetNumber(ws.Cell(rowIndex, col++), row.LabTotalHours);
                SetNumber(ws.Cell(rowIndex, col++), row.ConsultationHours);
                SetNumber(ws.Cell(rowIndex, col++), row.ExamHours);
                SetNumber(ws.Cell(rowIndex, col++), row.CreditHours);
                SetNumber(ws.Cell(rowIndex, col++), row.CourseWorkHours);
                SetNumber(ws.Cell(rowIndex, col++), row.CourseProjectHours);
                SetNumber(ws.Cell(rowIndex, col++), row.RgrHours);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), row.TotalHours);

                rowIndex++;
            }

            rowIndex = FillSectionHeader(ws, rowIndex, lastColumn, "ПРАКТИКИ");

            foreach (var row in practiceRows)
            {
                var col = 1;

                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = row.PracticeName;
                SetNumber(ws.Cell(rowIndex, col++), row.Course);
                SetNumber(ws.Cell(rowIndex, col++), row.StudentsCount);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), row.GroupCount);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), row.WeeksCount);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), row.TotalHours);

                rowIndex++;
            }

            rowIndex = FillSectionHeader(ws, rowIndex, lastColumn, "ГИА");

            foreach (var row in giaRows)
            {
                var col = 1;

                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
                ws.Cell(rowIndex, col++).Value = $"{row.GiaSection}: {row.WorkName}";
                SetNumber(ws.Cell(rowIndex, col++), row.Course);
                SetNumber(ws.Cell(rowIndex, col++), row.StudentsCount);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), row.GroupCount);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), 0);
                SetNumber(ws.Cell(rowIndex, col++), row.TotalHours);
                SetNumber(ws.Cell(rowIndex, col++), row.TotalHours);

                rowIndex++;
            }

            var lastUsedRow = rowIndex - 1;

            ApplyBorders(ws, 5, lastUsedRow, lastColumn);
            ApplyNumberFormat(ws, 6, lastUsedRow, 4, lastColumn);

            ws.Column(1).Width = 9;
            ws.Column(2).Width = 12;
            ws.Column(3).Width = 34;

            for (var column = 4; column <= lastColumn; column++)
            {
                ws.Column(column).Width = 8;
            }

            ws.Column(lastColumn).Width = 10;
            ws.Column(lastColumn).Style.Font.Bold = true;

            ws.SheetView.FreezeRows(5);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        private static int FillSectionHeader(
            IXLWorksheet ws,
            int rowIndex,
            int lastColumn,
            string title)
        {
            ws.Range(rowIndex, 1, rowIndex, lastColumn).Merge();

            var cell = ws.Cell(rowIndex, 1);

            cell.Value = title;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DDEBF7");
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            return rowIndex + 1;
        }

        private static void FillVerticalHeaders(
            IXLWorksheet ws,
            string[] headers)
        {
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);

                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.TextRotation = 90;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Row(1).Height = 95;
        }

        private static void FormatSimpleSheet(
            IXLWorksheet ws,
            int lastColumn,
            int lastRow)
        {
            if (lastRow < 1)
            {
                return;
            }

            var usedRange = ws.Range(1, 1, lastRow, lastColumn);

            usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            usedRange.Style.Alignment.WrapText = true;
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Column(1).Width = 9;
            ws.Column(2).Width = 12;
            ws.Column(3).Width = 34;

            for (var column = 4; column <= lastColumn; column++)
            {
                ws.Column(column).Width = 8;
            }

            ws.Column(lastColumn).Style.Font.Bold = true;
            ws.SheetView.FreezeRows(1);
        }

        private static void ApplyBorders(
            IXLWorksheet ws,
            int fromRow,
            int toRow,
            int lastColumn)
        {
            if (toRow < fromRow)
            {
                return;
            }

            var range = ws.Range(fromRow, 1, toRow, lastColumn);

            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Alignment.WrapText = true;
        }

        private static void ApplyNumberFormat(
            IXLWorksheet ws,
            int fromRow,
            int toRow,
            int fromColumn,
            int toColumn)
        {
            if (toRow < fromRow)
            {
                return;
            }

            for (var row = fromRow; row <= toRow; row++)
            {
                for (var column = fromColumn; column <= toColumn; column++)
                {
                    ws.Cell(row, column).Style.NumberFormat.Format = "0";
                }
            }
        }

        private static void SetNumber(
            IXLCell cell,
            decimal value)
        {
            cell.Value = value;
            cell.Style.NumberFormat.Format = "0";
        }

        private static void SetNumber(
            IXLCell cell,
            int value)
        {
            cell.Value = value;
            cell.Style.NumberFormat.Format = "0";
        }

        private static byte[] SaveWorkbook(XLWorkbook workbook)
        {
            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}