using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.NormTime;
using DepartmentLoadApp.Models.Workload;

namespace DepartmentLoadApp.Data
{
    public static class AppMemoryStore
    {
        public static ContingentPageViewModel Contingent { get; set; } = CreateContingent();
        public static List<NormTimeRowViewModel> NormTimes { get; set; } = CreateNormTimes();
        public static List<WorkloadTableRowViewModel> WorkloadRows { get; set; } = CreateWorkloadRows();

        private static ContingentPageViewModel CreateContingent()
        {
            return new ContingentPageViewModel
            {
                Rows = new List<ContingentDirectionRowViewModel>
                {
                    new()
                    {
                        DirectionCode = "09.03.04",
                        IsBachelor = true,
                        IsMaster = false,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    },
                    new()
                    {
                        DirectionCode = "09.03.03",
                        IsBachelor = true,
                        IsMaster = false,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    },
                    new()
                    {
                        DirectionCode = "09.04.04",
                        IsBachelor = false,
                        IsMaster = true,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    },
                    new()
                    {
                        DirectionCode = "09.04.03",
                        IsBachelor = false,
                        IsMaster = true,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    },
                    new()
                    {
                        DirectionCode = "09.04.03 БИ",
                        IsBachelor = false,
                        IsMaster = true,
                        Course1Count = 0,
                        Course2Count = 0,
                        Course3Count = 0,
                        Course4Count = 0
                    }
                }
            };
        }

        private static List<NormTimeRowViewModel> CreateNormTimes()
        {
            return new List<NormTimeRowViewModel>
            {
                new()
                {
                    Id = 1,
                    WorkTypeName = "Лекции",
                    UnitName = "час/поток",
                    CalculationType = NormCalculationType.PerFlow,
                    HoursValue = 1,
                    Note = "Для лекционных часов",
                    IsActive = true
                },
                new()
                {
                    Id = 2,
                    WorkTypeName = "Практические занятия",
                    UnitName = "час/группа",
                    CalculationType = NormCalculationType.PerGroup,
                    HoursValue = 1,
                    Note = "Для практических занятий",
                    IsActive = true
                },
                new()
                {
                    Id = 3,
                    WorkTypeName = "Лабораторные занятия",
                    UnitName = "час/подгруппа",
                    CalculationType = NormCalculationType.PerSubgroup,
                    HoursValue = 1,
                    Note = "Для лабораторных занятий",
                    IsActive = true
                }
            };
        }

        private static List<WorkloadTableRowViewModel> CreateWorkloadRows()
        {
            return new List<WorkloadTableRowViewModel>
            {
                new() { Id = 1, SemesterName = "осень", EducationForm = "очная", DirectionCode = "09.03.04", Course = 1 },
                new() { Id = 2, SemesterName = "осень", EducationForm = "очная", DirectionCode = "09.03.04", Course = 2 },
                new() { Id = 3, SemesterName = "осень", EducationForm = "очная", DirectionCode = "09.03.03", Course = 1 },
                new() { Id = 4, SemesterName = "весна", EducationForm = "очная", DirectionCode = "09.04.04", Course = 1 },
                new() { Id = 5, SemesterName = "весна", EducationForm = "очная", DirectionCode = "09.04.03", Course = 1 }
            };
        }
    }
}