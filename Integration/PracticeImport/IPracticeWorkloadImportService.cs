namespace DepartmentLoadApp.Integration.PracticeImport
{
    public interface IPracticeWorkloadImportService
    {
        Task EnsureYearImportedAsync(string year);
    }
}