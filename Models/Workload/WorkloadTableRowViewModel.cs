using System.ComponentModel.DataAnnotations;

namespace DepartmentLoadApp.Models.Workload
{
    public class WorkloadTableRowViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Семестр")]
        public string SemesterName { get; set; } = "осень";

        [Display(Name = "Форма")]
        public string EducationForm { get; set; } = "очная";

        [Display(Name = "Дисциплина по выбору")]
        public bool IsElectiveDiscipline { get; set; }

        [Display(Name = "Код направления")]
        public string DirectionCode { get; set; } = string.Empty;

        [Display(Name = "Полное наименование дисциплины")]
        public string DisciplineName { get; set; } = string.Empty;

        [Display(Name = "Курс")]
        public int Course { get; set; }

        [Display(Name = "Студентов")]
        public int StudentsCount { get; set; }

        [Display(Name = "Потоков")]
        public int FlowCount { get; set; }

        [Display(Name = "Групп")]
        public int GroupCount { get; set; }

        [Display(Name = "Подгрупп")]
        public int SubgroupCount { get; set; }

        [Display(Name = "Лекции: по плану часов")]
        public decimal LecturePlanHours { get; set; }

        [Display(Name = "Лекции: всего часов")]
        public decimal LectureTotalHours { get; set; }

        [Display(Name = "Практика: по плану часов")]
        public decimal PracticePlanHours { get; set; }

        [Display(Name = "Практика: всего часов")]
        public decimal PracticeTotalHours { get; set; }

        [Display(Name = "Лабораторные: по плану часов")]
        public decimal LabPlanHours { get; set; }

        [Display(Name = "Лабораторные: всего часов")]
        public decimal LabTotalHours { get; set; }

        public decimal TotalHours =>
            LectureTotalHours +
            PracticeTotalHours +
            LabTotalHours;
    }
}