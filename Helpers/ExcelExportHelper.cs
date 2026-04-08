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

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(1, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(1, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(1, i + 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

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

                for (int c = 1; c <= headers.Length; c++)
                {
                    ws.Cell(rowIndex, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(rowIndex, c).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(rowIndex, c).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                rowIndex++;
            }

            var usedRange = ws.Range(1, 1, Math.Max(1, rowIndex - 1), headers.Length);
            usedRange.Style.Alignment.WrapText = true;

            ws.Column(1).Width = 12;  // Семестр
            ws.Column(2).Width = 18;  // Форма обучения
            ws.Column(3).Width = 16;  // Код направления
            ws.Column(4).Width = 45;  // Наименование дисциплины
            ws.Column(5).Width = 10;  // Курс
            ws.Column(6).Width = 12;  // Студентов
            ws.Column(7).Width = 12;  // Потоков
            ws.Column(8).Width = 10;  // Групп
            ws.Column(9).Width = 12;  // Подгрупп

            ws.Column(10).Width = 16; // Лекции план
            ws.Column(11).Width = 16; // Лекции всего
            ws.Column(12).Width = 22; // Практ план
            ws.Column(13).Width = 22; // Практ всего
            ws.Column(14).Width = 22; // Лаб план
            ws.Column(15).Width = 22; // Лаб всего

            ws.Column(16).Width = 12; // Экзамен
            ws.Column(17).Width = 12; // Зачет
            ws.Column(18).Width = 18; // Курсовая работа
            ws.Column(19).Width = 18; // Курсовой проект
            ws.Column(20).Width = 12; // РГР

            ws.Column(21).Width = 16; // Экзамен часы
            ws.Column(22).Width = 16; // Зачет часы
            ws.Column(23).Width = 22; // Курсовая работа часы
            ws.Column(24).Width = 22; // Курсовой проект часы
            ws.Column(25).Width = 16; // РГР часы
            ws.Column(26).Width = 16; // Консультации
            ws.Column(27).Width = 14; // Итого

            ws.SheetView.FreezeRows(1);
            ws.Range(1, 1, 1, headers.Length).SetAutoFilter();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
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