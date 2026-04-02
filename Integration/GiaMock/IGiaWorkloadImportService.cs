namespace DepartmentLoadApp.Integration.GiaMock
{
    public interface IGiaWorkloadImportService
    {
        Task EnsureYearImportedAsync(int year);
    }
}