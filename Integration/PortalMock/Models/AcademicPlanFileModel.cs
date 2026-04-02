namespace DepartmentLoadApp.Integration.PortalMock.Models
{
    public class AcademicPlanFileModel
    {
        public List<AcademicPlanModel> AcademicPlans { get; set; }
    }

    public class AcademicPlanModel
    {
        public string Year { get; set; }
        public int Id { get; set; }
    }
}