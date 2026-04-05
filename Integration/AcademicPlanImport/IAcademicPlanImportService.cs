namespace DepartmentLoadApp.Integration.AcademicPlanImport
{
    public interface IAcademicPlanImportService
    {
        Task<string?> GetLatestYearAsync();
        Task EnsureYearImportedAsync(string year);
        Task ImportYearAsync(string year);
    }
}