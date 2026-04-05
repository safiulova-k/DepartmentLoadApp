namespace DepartmentLoadApp.Integration.AcademicPlanImport.Models
{
    public class AcademicPlanFileModel
    {
        public List<AcademicPlanModel> AcademicPlans { get; set; } = new();
    }

    public class AcademicPlanModel
    {
        public string Year { get; set; } = string.Empty;
        public int Id { get; set; }
    }
}