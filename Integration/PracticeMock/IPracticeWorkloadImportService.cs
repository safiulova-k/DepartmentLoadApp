namespace DepartmentLoadApp.Integration.PracticeMock
{
    public interface IPracticeWorkloadImportService
    {
        Task EnsureYearImportedAsync(int year);
    }
}