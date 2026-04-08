using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class StudentGroupSyncService
{
    private readonly CoreApiService _api;
    private readonly DepartmentLoadDbContext _db;

    public StudentGroupSyncService(CoreApiService api, DepartmentLoadDbContext db)
    {
        _api = api;
        _db = db;
    }

    public async Task Sync()
    {
        var items = await _api.GetListAsync<StudentGroupDto>("StudentGroups/get-all");

        foreach (var dto in items)
        {
            var entity = await _db.Set<StudentGroup>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            var direction = await _db.Set<EducationDirection>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.EducationDirectionId);

            var curator = dto.CuratorId.HasValue
                ? await _db.Set<Lecturer>().FirstOrDefaultAsync(x => x.CoreId == dto.CuratorId)
                : null;

            if (entity == null)
            {
                _db.Add(new StudentGroup
                {
                    CoreId = dto.Id,
                    GroupName = dto.GroupName,
                    Course = dto.Course,
                    EducationDirectionId = direction!.Id,
                    CuratorId = curator?.Id
                });
            }
            else
            {
                entity.GroupName = dto.GroupName;
                entity.Course = dto.Course;
                entity.EducationDirectionId = direction!.Id;
                entity.CuratorId = curator?.Id;
            }
        }

        await _db.SaveChangesAsync();
    }
}