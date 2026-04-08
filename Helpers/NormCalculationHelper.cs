using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.Helpers
{
    public static class NormCalculationHelper
    {
        public static decimal GetBaseValue(WorkCalculationBase calculationBase, WorkloadRow row)
        {
            return calculationBase switch
            {
                WorkCalculationBase.PerStream => row.FlowCount,
                WorkCalculationBase.PerGroup => row.GroupCount,
                WorkCalculationBase.PerSubgroup => row.SubgroupCount,
                WorkCalculationBase.PerStudent => row.StudentsCount,
                WorkCalculationBase.PerWork => 1,
                WorkCalculationBase.FromLectureHoursTotal => row.GroupCount * row.LecturePlanHours,
                _ => 0
            };
        }

        public static decimal CalculatePlanHours(decimal planHours, NormTime? norm, WorkloadRow row)
        {
            if (planHours <= 0 || norm == null)
            {
                return 0;
            }

            var baseValue = GetBaseValue(norm.CalculationBase, row);
            return CalculationHelper.RoundHours(planHours * baseValue * norm.Hours);
        }

        public static decimal CalculateOptionalHours(bool enabled, NormTime? norm, WorkloadRow row)
        {
            if (!enabled || norm == null)
            {
                return 0;
            }

            var baseValue = GetBaseValue(norm.CalculationBase, row);
            return CalculationHelper.RoundHours(baseValue * norm.Hours);
        }

        public static decimal CalculateConsultationHours(
            WorkloadRow row,
            NormTime? consultationNorm,
            NormTime? consultationExamExtraNorm)
        {
            decimal result = 0;

            if (consultationNorm != null)
            {
                decimal consultationBase = consultationNorm.CalculationBase switch
                {
                    WorkCalculationBase.FromLectureHoursTotal => row.GroupCount * row.LecturePlanHours,
                    _ => GetBaseValue(consultationNorm.CalculationBase, row)
                };

                result += consultationBase * consultationNorm.Hours;
            }

            if (row.HasExam && consultationExamExtraNorm != null)
            {
                var extraBase = GetBaseValue(consultationExamExtraNorm.CalculationBase, row);
                result += extraBase * consultationExamExtraNorm.Hours;
            }

            return CalculationHelper.RoundHours(result);
        }
    }
}