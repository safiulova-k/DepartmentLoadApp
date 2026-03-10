using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.Controllers
{
    public class WorkloadController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var model = CreateDefaultModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(WorkloadTablePageViewModel model)
        {
            EnsureCollections(model);
            RecalculateAll(model);
            ModelState.Clear();
            return View(model);
        }

        [HttpPost]
        public IActionResult Calculate(WorkloadTablePageViewModel model)
        {
            EnsureCollections(model);
            RecalculateAll(model);
            ModelState.Clear();
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult AddDisciplineRow(WorkloadTablePageViewModel model)
        {
            EnsureCollections(model);
            model.DisciplineRows.Add(CreateRow(GetNextId(model)));
            return View("Index", model);
        }

        private static WorkloadTablePageViewModel CreateDefaultModel()
        {
            var model = new WorkloadTablePageViewModel();
            model.DisciplineRows.Add(CreateRow(1));
            return model;
        }

        private static void EnsureCollections(WorkloadTablePageViewModel model)
        {
            model.DisciplineRows ??= new();
        }

        private static WorkloadTableRowViewModel CreateRow(int id)
        {
            return new WorkloadTableRowViewModel
            {
                Id = id,
                DisplayOrder = id,
                IsSectionHeader = false,
                SectionType = WorkloadSectionType.Disciplines,
                RowKind = WorkloadRowKind.Discipline,
                FormulaType = WorkloadFormulaType.Discipline,
                SectionTitle = null,
                Semester = null,
                EducationForm = string.Empty,
                IsElectiveDiscipline = false,
                DirectionCode = string.Empty,
                DisciplineName = string.Empty,
                Course = null,
                StudentsCount = 0,
                StreamsCount = 0,
                GroupsCount = 0,
                SubGroupsCount = 0,
                LecturePlanHours = 0,
                LectureTotalHours = 0,
                PracticePlanHours = 0,
                PracticeTotalHours = 0,
                LaboratoryPlanHours = 0,
                LaboratoryTotalHours = 0,
                HasExamInPlan = false,
                HasCreditInPlan = false,
                HasCourseWorkInPlan = false,
                HasCourseProjectInPlan = false,
                ConsultationHours = 0,
                ExamHours = 0,
                CreditHours = 0,
                CourseWorkHours = 0,
                CourseProjectHours = 0,
                StateExamHours = 0,
                DiplomaProjectHours = 0,
                GekHours = 0,
                OrganizationalWorkHours = 0,
                StudyPracticeWeeks = 0,
                PracticeHours = 0,
                PrediplomaPracticeHours = 0,
                ScientificPedagogicalPracticeValue = 0,
                ResearchGuidanceHours = 0,
                AbstractOrRgrHours = 0,
                TotalHours = 0
            };
        }

        private static int GetNextId(WorkloadTablePageViewModel model)
        {
            return model.DisciplineRows.Any() ? model.DisciplineRows.Max(x => x.Id) + 1 : 1;
        }

        private static void RecalculateAll(WorkloadTablePageViewModel model)
        {
            foreach (var row in model.DisciplineRows)
            {
                RecalculateRow(row);
            }
        }

        private static void RecalculateRow(WorkloadTableRowViewModel row)
        {
            var students = row.StudentsCount ?? 0;
            var groups = row.GroupsCount ?? 0;
            var subGroups = row.SubGroupsCount ?? 0;

            row.FormulaType = WorkloadFormulaType.Discipline;
            row.RowKind = WorkloadRowKind.Discipline;
            row.SectionType = WorkloadSectionType.Disciplines;

            row.LectureTotalHours = row.LecturePlanHours;
            row.PracticeTotalHours = row.PracticePlanHours * groups;
            row.LaboratoryTotalHours = row.LaboratoryPlanHours * subGroups;

            row.ConsultationHours =
                RoundToHalf(0.05m * groups * row.LecturePlanHours) +
                (row.HasExamInPlan ? 2m * groups : 0m);

            row.ExamHours = row.HasExamInPlan ? students * 0.3m : 0m;
            row.CreditHours = row.HasCreditInPlan ? students * 0.2m : 0m;
            row.CourseWorkHours = row.HasCourseWorkInPlan ? students * 1.5m : 0m;
            row.CourseProjectHours = row.HasCourseProjectInPlan ? students * 3m : 0m;

            row.TotalHours =
                row.LectureTotalHours +
                row.PracticeTotalHours +
                row.LaboratoryTotalHours +
                row.ConsultationHours +
                row.ExamHours +
                row.CreditHours +
                row.CourseWorkHours +
                row.CourseProjectHours +
                row.OrganizationalWorkHours +
                row.ResearchGuidanceHours +
                row.AbstractOrRgrHours;
        }

        private static decimal RoundToHalf(decimal value)
        {
            return Math.Round(value * 2m, 0, MidpointRounding.AwayFromZero) / 2m;
        }
    }
}