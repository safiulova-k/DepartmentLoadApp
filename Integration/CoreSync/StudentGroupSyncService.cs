using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class StudentGroupSyncService : IStudentGroupSyncService
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
        var items = await _api.GetListAsync<StudentGroupDto>("StudentGroups/GetStudentGroupList");

        foreach (var dto in items)
        {
            var entity = await _db.StudentGroupsCore
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            var direction = await _db.EducationDirections
                .FirstOrDefaultAsync(x => x.CoreId == dto.EducationDirectionId);

            if (direction == null)
            {
                continue;
            }

            Lecturer? curator = null;
            if (dto.CuratorId.HasValue)
            {
                curator = await _db.Lecturers
                    .FirstOrDefaultAsync(x => x.CoreId == dto.CuratorId.Value);
            }

            if (entity == null)
            {
                entity = new StudentGroup
                {
                    CoreId = dto.Id,
                    EducationDirectionId = direction.Id,
                    CuratorId = curator?.Id,
                    GroupName = dto.GroupName,
                    Course = dto.Course
                };

                _db.StudentGroupsCore.Add(entity);
            }
            else
            {
                entity.EducationDirectionId = direction.Id;
                entity.CuratorId = curator?.Id;
                entity.GroupName = dto.GroupName;
                entity.Course = dto.Course;
            }
        }

        await _db.SaveChangesAsync();
    }
}