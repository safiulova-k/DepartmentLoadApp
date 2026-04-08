using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class AcademicPlanSyncService : IAcademicPlanSyncService
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
        var items = await _api.GetListAsync<AcademicPlanDto>("AcademicPlans/GetAcademicPlanList");

        foreach (var dto in items)
        {
            var entity = await _db.AcademicPlansCore
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            int? localEducationDirectionId = null;
            if (dto.EducationDirectionId.HasValue)
            {
                var direction = await _db.EducationDirections
                    .FirstOrDefaultAsync(x => x.CoreId == dto.EducationDirectionId.Value);

                if (direction == null)
                {
                    continue;
                }

                localEducationDirectionId = direction.Id;
            }

            if (entity == null)
            {
                entity = new AcademicPlan
                {
                    CoreId = dto.Id,
                    EducationDirectionId = localEducationDirectionId,
                    EducationForm = dto.EducationForm,
                    AcademicCourses = dto.AcademicCourses,
                    Year = dto.Year
                };

                _db.AcademicPlansCore.Add(entity);
            }
            else
            {
                entity.EducationDirectionId = localEducationDirectionId;
                entity.EducationForm = dto.EducationForm;
                entity.AcademicCourses = dto.AcademicCourses;
                entity.Year = dto.Year;
            }
        }

        await _db.SaveChangesAsync();
    }
}