using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class AcademicPlanSyncService
{
    private readonly CoreApiService _api;
    private readonly DepartmentLoadDbContext _db;

    public AcademicPlanSyncService(CoreApiService api, DepartmentLoadDbContext db)
    {
        _api = api;
        _db = db;
    }

    public async Task Sync()
    {
        var items = await _api.GetListAsync<AcademicPlanDto>("AcademicPlans/get-all");

        foreach (var dto in items)
        {
            var entity = await _db.Set<AcademicPlan>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            var direction = await _db.Set<EducationDirection>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.EducationDirectionId);

            if (entity == null)
            {
                _db.Add(new AcademicPlan
                {
                    CoreId = dto.Id,
                    EducationDirectionId = direction!.Id,
                    EducationForm = dto.EducationForm,
                    AcademicCourses = dto.AcademicCourses,
                    Year = dto.Year
                });
            }
            else
            {
                entity.EducationDirectionId = direction!.Id;
                entity.EducationForm = dto.EducationForm;
                entity.AcademicCourses = dto.AcademicCourses;
                entity.Year = dto.Year;
            }
        }

        await _db.SaveChangesAsync();
    }
}