using System;
using System.Linq;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Workload;
using Microsoft.AspNetCore.Mvc;

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
            return View(model);
        }

        [HttpPost]
        public IActionResult Calculate(WorkloadTablePageViewModel model)
        {
            EnsureCollections(model);
            RecalculateAll(model);
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult AddGuidanceRow(WorkloadTablePageViewModel model)
        {
            EnsureCollections(model);
            model.GuidanceRows.Add(CreateRow(
                WorkloadSectionType.Guidance,
                WorkloadRowKind.Custom,
                WorkloadFormulaType.Custom,
                GetNextId(model)));
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult AddGiaRow(WorkloadTablePageViewModel model)
        {
            EnsureCollections(model);
            model.GiaRows.Add(CreateRow(
                WorkloadSectionType.Gia,
                WorkloadRowKind.Custom,
                WorkloadFormulaType.Custom,
                GetNextId(model)));
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult AddPracticeRow(WorkloadTablePageViewModel model)
        {
            EnsureCollections(model);
            model.PracticeRows.Add(CreateRow(
                WorkloadSectionType.Practice,
                WorkloadRowKind.Custom,
                WorkloadFormulaType.Custom,
                GetNextId(model)));
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult AddDisciplineRow(WorkloadTablePageViewModel model)
        {
            EnsureCollections(model);
            model.DisciplineRows.Add(CreateRow(
                WorkloadSectionType.Disciplines,
                WorkloadRowKind.Discipline,
                WorkloadFormulaType.Discipline,
                GetNextId(model)));
            return View("Index", model);
        }

        private static WorkloadTablePageViewModel CreateDefaultModel()
        {
            var model = new WorkloadTablePageViewModel();

            model.DisciplineRows.Add(CreateRow(
                WorkloadSectionType.Disciplines,
                WorkloadRowKind.Discipline,
                WorkloadFormulaType.Discipline,
                1));

            return model;
        }

        private static void EnsureCollections(WorkloadTablePageViewModel model)
        {
            model.GuidanceRows ??= new();
            model.GiaRows ??= new();
            model.PracticeRows ??= new();
            model.DisciplineRows ??= new();
        }

        private static void RecalculateAll(WorkloadTablePageViewModel model)
        {
            RecalculateCollection(model.GuidanceRows);
            RecalculateCollection(model.GiaRows);
            RecalculateCollection(model.PracticeRows);
            RecalculateCollection(model.DisciplineRows);
        }

        private static WorkloadTableRowViewModel CreateRow(
            WorkloadSectionType sectionType,
            WorkloadRowKind rowKind,
            WorkloadFormulaType formulaType,
            int id)
        {
            return new WorkloadTableRowViewModel
            {
                Id = id,
                DisplayOrder = id,
                IsSectionHeader = false,
                SectionType = sectionType,
                RowKind = rowKind,
                FormulaType = formulaType,
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
                PracticePlanHours = 0,
                LaboratoryPlanHours = 0,
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
            var allIds = model.GuidanceRows.Select(x => x.Id)
                .Concat(model.GiaRows.Select(x => x.Id))
                .Concat(model.PracticeRows.Select(x => x.Id))
                .Concat(model.DisciplineRows.Select(x => x.Id));

            return allIds.Any() ? allIds.Max() + 1 : 1;
        }

        private static void RecalculateCollection(System.Collections.Generic.List<WorkloadTableRowViewModel> rows)
        {
            foreach (var row in rows)
            {
                RecalculateRow(row);
            }
        }

        private static void RecalculateRow(WorkloadTableRowViewModel row)
        {
            var students = row.StudentsCount ?? 0;
            var groups = row.GroupsCount ?? 0;
            var subGroups = row.SubGroupsCount ?? 0;

            row.LectureTotalHours = 0;
            row.PracticeTotalHours = 0;
            row.LaboratoryTotalHours = 0;
            row.ConsultationHours = 0;
            row.ExamHours = 0;
            row.CreditHours = 0;
            row.CourseWorkHours = 0;
            row.CourseProjectHours = 0;
            row.StateExamHours = 0;
            row.DiplomaProjectHours = 0;
            row.GekHours = 0;
            row.TotalHours = 0;

            switch (row.FormulaType)
            {
                case WorkloadFormulaType.Discipline:
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
                    break;

                case WorkloadFormulaType.BachelorThesisGuidance:
                    row.DiplomaProjectHours = students * 10m + students * 0.5m;
                    row.TotalHours = row.DiplomaProjectHours;
                    break;

                case WorkloadFormulaType.SpecialistThesisGuidance:
                    row.DiplomaProjectHours = students * 15m + students * 0.5m;
                    row.TotalHours = row.DiplomaProjectHours;
                    break;

                case WorkloadFormulaType.MasterThesisGuidance:
                    row.DiplomaProjectHours = students * 30m + students * 0.5m;
                    row.TotalHours = row.DiplomaProjectHours;
                    break;

                case WorkloadFormulaType.ThesisReview:
                    row.DiplomaProjectHours = students * 2m;
                    row.TotalHours = row.DiplomaProjectHours;
                    break;

                case WorkloadFormulaType.ThesisPreReview:
                    row.DiplomaProjectHours = students * 0.5m;
                    row.TotalHours = row.DiplomaProjectHours;
                    break;

                case WorkloadFormulaType.NormControl:
                    row.DiplomaProjectHours = students * 1m;
                    row.TotalHours = row.DiplomaProjectHours;
                    break;

                case WorkloadFormulaType.StateExam:
                    row.StateExamHours = students * (0.5m * 5m + 1m);
                    row.TotalHours = row.StateExamHours;
                    break;

                case WorkloadFormulaType.GekWork:
                    row.GekHours = students * (0.5m * 5m + 1m);
                    row.TotalHours = row.GekHours;
                    break;

                case WorkloadFormulaType.StudyPractice:
                    row.TotalHours = students * row.StudyPracticeWeeks;
                    break;

                case WorkloadFormulaType.ProductionPractice:
                    row.TotalHours = groups * row.PracticeHours * 10m;
                    break;

                case WorkloadFormulaType.PrediplomaPractice:
                    row.TotalHours = students * row.PrediplomaPracticeHours;
                    break;

                case WorkloadFormulaType.ScientificPedagogicalPractice:
                    row.TotalHours = students * row.ScientificPedagogicalPracticeValue;
                    break;

                case WorkloadFormulaType.ResearchWork:
                    row.TotalHours = students * 1.3m * row.PracticeHours;
                    break;

                case WorkloadFormulaType.Custom:
                    row.TotalHours =
                        row.OrganizationalWorkHours +
                        row.StudyPracticeWeeks +
                        row.PracticeHours +
                        row.PrediplomaPracticeHours +
                        row.ScientificPedagogicalPracticeValue +
                        row.ResearchGuidanceHours +
                        row.AbstractOrRgrHours;
                    break;

                case WorkloadFormulaType.None:
                default:
                    row.TotalHours = 0;
                    break;
            }
        }

        private static decimal RoundToHalf(decimal value)
        {
            return Math.Round(value * 2m, 0, MidpointRounding.AwayFromZero) / 2m;
        }
    }
}