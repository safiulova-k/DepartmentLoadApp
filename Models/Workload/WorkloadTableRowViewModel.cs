using DepartmentLoadApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DepartmentLoadApp.Models.Workload
{
    public class WorkloadTableRowViewModel
    {
        public int Id { get; set; }
        public int DisplayOrder { get; set; }

        public bool IsSectionHeader { get; set; }

        public WorkloadSectionType SectionType { get; set; }

        public WorkloadRowKind RowKind { get; set; }
        public WorkloadFormulaType FormulaType { get; set; }

        public string? SectionTitle { get; set; }

        [Display(Name = "Семестр")]
        public SemesterType? Semester { get; set; }

        [Display(Name = "Форма обучения")]
        public string? EducationForm { get; set; }

        [Display(Name = "Дисциплина по выбору")]
        public bool IsElectiveDiscipline { get; set; }

        [Display(Name = "Код направления(специальности) по ФГОС 3+")]
        public string? DirectionCode { get; set; }

        [Display(Name = "Полное наименование дисциплин")]
        public string? DisciplineName { get; set; }

        [Display(Name = "Курс")]
        public int? Course { get; set; }

        [Display(Name = "Студентов")]
        public int? StudentsCount { get; set; }

        [Display(Name = "Потоков")]
        public int? StreamsCount { get; set; }

        [Display(Name = "Групп")]
        public int? GroupsCount { get; set; }

        [Display(Name = "Подгрупп")]
        public int? SubGroupsCount { get; set; }

        [Display(Name = "Лекции: по плану часов")]
        public decimal LecturePlanHours { get; set; }

        [Display(Name = "Лекции: всего часов")]
        public decimal LectureTotalHours { get; set; }

        [Display(Name = "Практ. семин.: по плану часов")]
        public decimal PracticePlanHours { get; set; }

        [Display(Name = "Практ. семин.: всего часов")]
        public decimal PracticeTotalHours { get; set; }

        [Display(Name = "Лаборат. занятия: по плану часов")]
        public decimal LaboratoryPlanHours { get; set; }

        [Display(Name = "Лаборат. занятия: всего часов")]
        public decimal LaboratoryTotalHours { get; set; }

        [Display(Name = "По учебному плану: экзамен")]
        public bool HasExamInPlan { get; set; }

        [Display(Name = "По учебному плану: зачет")]
        public bool HasCreditInPlan { get; set; }

        [Display(Name = "По учебному плану: курсовая работа")]
        public bool HasCourseWorkInPlan { get; set; }

        [Display(Name = "По учебному плану: курсовой проект")]
        public bool HasCourseProjectInPlan { get; set; }

        [Display(Name = "Консультации")]
        public decimal ConsultationHours { get; set; }

        [Display(Name = "Экзамен")]
        public decimal ExamHours { get; set; }

        [Display(Name = "Зачет")]
        public decimal CreditHours { get; set; }

        [Display(Name = "Курсовая работа")]
        public decimal CourseWorkHours { get; set; }

        [Display(Name = "Курсовой проект")]
        public decimal CourseProjectHours { get; set; }

        [Display(Name = "Госэкзамен")]
        public decimal StateExamHours { get; set; }

        [Display(Name = "Дипломное проектирование")]
        public decimal DiplomaProjectHours { get; set; }

        [Display(Name = "ГЭК")]
        public decimal GekHours { get; set; }

        [Display(Name = "Орг.работа")]
        public decimal OrganizationalWorkHours { get; set; }

        [Display(Name = "Учебная практика(недели)")]
        public decimal StudyPracticeWeeks { get; set; }

        [Display(Name = "Практика")]
        public decimal PracticeHours { get; set; }

        [Display(Name = "Преддипломная практика")]
        public decimal PrediplomaPracticeHours { get; set; }

        [Display(Name = "Научно-педагогич.практика зет")]
        public decimal ScientificPedagogicalPracticeValue { get; set; }

        [Display(Name = "Руков. научной работой")]
        public decimal ResearchGuidanceHours { get; set; }

        [Display(Name = "Рефераты/РГР")]
        public decimal AbstractOrRgrHours { get; set; }

        [Display(Name = "Итого")]
        public decimal TotalHours { get; set; }
    }
}
