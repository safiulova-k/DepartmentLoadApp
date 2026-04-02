namespace DepartmentLoadApp.Integration.PortalMock
{
    public interface IAcademicPlanImportService
    {
        Task ImportYearAsync(string year);
        Task<int?> GetLatestYearAsync();
        Task EnsureYearImportedAsync(string year);
    }
}