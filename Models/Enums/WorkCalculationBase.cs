namespace DepartmentLoadApp.Models.Enums
{
    public enum WorkCalculationBase
    {
        PerStream = 0,      // на поток
        PerGroup = 1,       // на группу
        PerSubgroup = 2,    // на подгруппу
        PerStudent = 3,     // на студента
        PerWork = 4,        // за одну работу
        FromLectureHoursTotal = 5       // от общего числа лекционных часов (для консультаций)
    }
}