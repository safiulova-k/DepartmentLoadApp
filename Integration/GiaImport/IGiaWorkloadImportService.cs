namespace DepartmentLoadApp.Integration.GiaImport
{
    public interface IGiaWorkloadImportService
    {
        Task EnsureYearImportedAsync(string year);
    }
}