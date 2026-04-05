using ClosedXML.Excel;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.Helpers
{
    public static class ExcelExportHelper
    {
        public static byte[] ExportWorkload(IEnumerable<WorkloadRow> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Дисциплины");

            var headers = new[]
            {
                "Дисциплина", "Семестр", "Форма обучения", "Код направления", "Курс",
                "Студентов", "Потоков", "Групп", "Подгрупп",
                "Лекции (план)", "Лекции (всего)",
                "Практика (план)", "Практика (всего)",
                "Лаб. (план)", "Лаб. (всего)",
                "Экзамен", "Зачет", "Курсовая работа", "Курсовой проект",
                "Консультации",
                "Экзамен (часы)", "Зачет (часы)",
                "Курсовая работа (часы)", "Курсовой проект (часы)",
                "Итого"
            };

            FillHeaders(ws, headers);

            var rowIndex = 2;
            foreach (var row in rows)
            {
                var col = 1;

                ws.Cell(rowIndex, col++).Value = row.DisciplineName;
                ws.Cell(rowIndex, col++).Value = row.SemesterName;
                ws.Cell(rowIndex, col++).Value = row.EducationForm;
                ws.Cell(rowIndex, col++).Value = row.DirectionCode;
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
                ws.Cell(rowIndex, col++).Value = row.ConsultationHours;
                ws.Cell(rowIndex, col++).Value = row.ExamHours;
                ws.Cell(rowIndex, col++).Value = row.CreditHours;
                ws.Cell(rowIndex, col++).Value = row.CourseWorkHours;
                ws.Cell(rowIndex, col++).Value = row.CourseProjectHours;
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
                "Семестр", "Форма обучения", "Код направления", "Вид практики",
                "Курс", "Групп", "Студентов", "Количество недель", "Итого"
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
                "Раздел", "Семестр", "Форма обучения", "Код направления",
                "Вид работы", "Курс", "Групп", "Студентов",
                "Часы вручную", "Итого"
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

        private static void FillHeaders(IXLWorksheet ws, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
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
            }
        }

        private static byte[] SaveWorkbook(XLWorkbook workbook)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}