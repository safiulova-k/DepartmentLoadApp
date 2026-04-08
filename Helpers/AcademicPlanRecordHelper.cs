using DepartmentLoadApp.Models.Core;
using DepartmentLoadApp.Models.Core.Enums;

namespace DepartmentLoadApp.Helpers
{
    public static class AcademicPlanRecordHelper
    {
        public static bool IsDiscipline(AcademicPlanRecord record)
        {
            var block = GetTopLevelBlock(record.Index);
            return block == "Б1" || block == "ФТД";
        }

        public static bool IsPractice(AcademicPlanRecord record)
        {
            return GetTopLevelBlock(record.Index) == "Б2";
        }

        public static bool IsGia(AcademicPlanRecord record)
        {
            return GetTopLevelBlock(record.Index) == "Б3";
        }

        public static string GetTopLevelBlock(string? index)
        {
            if (string.IsNullOrWhiteSpace(index))
            {
                return string.Empty;
            }

            var normalized = index.Trim().ToUpperInvariant();

            if (normalized.StartsWith("ФТД"))
            {
                return "ФТД";
            }

            var parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return $"{parts[0]}.{parts[1]}";
            }

            return normalized;
        }

        public static bool IsFacultyOptionalByIndex(string? index)
        {
            return GetTopLevelBlock(index) == "ФТД";
        }

        public static string GetSemesterName(int semester)
        {
            return semester % 2 == 0 ? "Весна" : "Осень";
        }

        public static int GetCourseFromSemester(int semester)
        {
            if (semester <= 0)
            {
                return 0;
            }

            return (semester + 1) / 2;
        }

        public static string GetEducationFormName(EducationForm educationForm)
        {
            return educationForm switch
            {
                EducationForm.Очная => "Очная",
                EducationForm.Заочная => "Заочная",
                _ => educationForm.ToString()
            };
        }

        public static bool HasValue(int? value)
        {
            return value.HasValue && value.Value > 0;
        }
    }
}