using DepartmentLoadApp.Models.Core;

namespace DepartmentLoadApp.Models;

public class LoadDistribution
{
    public int Id { get; set; }

    public int LecturerId { get; set; }
    public Lecturer? Lecturer { get; set; }

    public int LoadCalculationId { get; set; }
    public LoadCalculation? LoadCalculation { get; set; }

    public decimal Hours { get; set; }
}