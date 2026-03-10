using DepartmentLoadApp.Interfaces;
using DepartmentLoadApp.ViewModels;

namespace DepartmentLoadApp.Services
{
    public class WorkloadCalculationService : IWorkloadCalculationService
    {
        private const decimal ConsultationPercentFromLecture = 0.05m;
        private const decimal ExamPerStudent = 0.30m;
        private const decimal CreditPerStudent = 0.20m;
        private const decimal CourseWorkPerStudent = 0.75m;
        private const decimal CourseProjectPerStudent = 1.00m;
        private const decimal ReferatRgrPerStudent = 0.30m;

        private const decimal DiplomaFixedHours = 10m;
        private const decimal GekFixedHours = 8m;
        private const decimal PracticeManagementFixedHours = 6m;
        private const decimal EducationalPracticeFixedHours = 6m;
        private const decimal IndustrialPracticeFixedHours = 6m;
        private const decimal PreDiplomaPracticeFixedHours = 6m;
        private const decimal ScientificPedagogicalPracticeFixedHours = 6m;
        private const decimal ScientificQualificationWorkFixedHours = 12m;
        private const decimal ParticipationInGekFixedHours = 4m;

        public WorkloadCalculationPageViewModel Calculate(WorkloadCalculationPageViewModel model)
        {
            if (model.Rows == null)
            {
                model.Rows = new List<WorkloadCalculationRowViewModel>();
                return model;
            }

            foreach (var row in model.Rows)
            {
                row.LectureTotalHours = row.LecturePlanHours * Math.Max(row.StreamCount, 1);
                row.PracticeTotalHours = row.PracticePlanHours * Math.Max(row.GroupCount, 1);
                row.LaboratoryTotalHours = row.LaboratoryPlanHours * Math.Max(row.SubGroupCount, 1);

                row.ConsultationHours = row.HasConsultation
                    ? CalculateConsultationHours(row.LecturePlanHours, row.GroupCount)
                    : null;

                row.ExamHours = row.HasExam
                    ? row.StudentCount * ExamPerStudent
                    : null;

                row.CreditHours = row.HasCredit
                    ? row.StudentCount * CreditPerStudent
                    : null;

                row.CourseWorkHours = row.HasCourseWork
                    ? row.StudentCount * CourseWorkPerStudent
                    : null;

                row.CourseProjectHours = row.HasCourseProject
                    ? row.StudentCount * CourseProjectPerStudent
                    : null;

                row.ReferatRgrHours = row.HasReferatRgr
                    ? row.StudentCount * ReferatRgrPerStudent
                    : null;

                row.DiplomaHours = row.HasDiploma ? DiplomaFixedHours : null;
                row.GekHours = row.HasGek ? GekFixedHours : null;
                row.PracticeManagementHours = row.HasPracticeManagement ? PracticeManagementFixedHours : null;
                row.EducationalPracticeHours = row.HasEducationalPractice ? EducationalPracticeFixedHours : null;
                row.IndustrialPracticeHours = row.HasIndustrialPractice ? IndustrialPracticeFixedHours : null;
                row.PreDiplomaPracticeHours = row.HasPreDiplomaPractice ? PreDiplomaPracticeFixedHours : null;
                row.ScientificPedagogicalPracticeHours = row.HasScientificPedagogicalPractice ? ScientificPedagogicalPracticeFixedHours : null;
                row.ScientificQualificationWorkHours = row.HasScientificQualificationWork ? ScientificQualificationWorkFixedHours : null;
                row.ParticipationInGekHours = row.HasParticipationInGek ? ParticipationInGekFixedHours : null;

                row.TotalHours =
                    row.LectureTotalHours +
                    row.PracticeTotalHours +
                    row.LaboratoryTotalHours +
                    (row.ConsultationHours ?? 0) +
                    (row.ExamHours ?? 0) +
                    (row.CreditHours ?? 0) +
                    (row.CourseWorkHours ?? 0) +
                    (row.CourseProjectHours ?? 0) +
                    (row.ReferatRgrHours ?? 0) +
                    (row.DiplomaHours ?? 0) +
                    (row.GekHours ?? 0) +
                    (row.PracticeManagementHours ?? 0) +
                    (row.EducationalPracticeHours ?? 0) +
                    (row.IndustrialPracticeHours ?? 0) +
                    (row.PreDiplomaPracticeHours ?? 0) +
                    (row.ScientificPedagogicalPracticeHours ?? 0) +
                    (row.ScientificQualificationWorkHours ?? 0) +
                    (row.ParticipationInGekHours ?? 0);
            }

            return model;
        }

        private decimal CalculateConsultationHours(decimal lecturePlanHours, int groupCount)
        {
            return (lecturePlanHours * ConsultationPercentFromLecture) + (2 * Math.Max(groupCount, 1));
        }
    }
}