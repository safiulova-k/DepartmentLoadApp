namespace DepartmentLoadApp.Integration.PortalMock
{
    public interface IAcademicPlanImportService
    {
        Task<int?> GetLatestYearAsync();

        Task EnsureYearImportedAsync(int year);

        Task ImportYearAsync(int year);
    }
}