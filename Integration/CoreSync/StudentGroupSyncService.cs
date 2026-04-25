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
        var groupDtos = await _api.GetListAsync<StudentGroupDto>("StudentGroups/GetStudentGroupList");
        var studentDtos = await _api.GetListAsync<StudentDto>("Students/GetStudentList");

        var studentCountsByCoreGroupId = studentDtos
            .Where(x => x.StudentGroupId.HasValue)
            .GroupBy(x => x.StudentGroupId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var dto in groupDtos)
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

            var studentCount = studentCountsByCoreGroupId.TryGetValue(dto.Id, out var count)
                ? count
                : 0;

            if (entity == null)
            {
                entity = new StudentGroup
                {
                    CoreId = dto.Id,
                    EducationDirectionId = direction.Id,
                    CuratorId = curator?.Id,
                    GroupName = dto.GroupName,
                    Course = dto.Course,
                    StudentCount = studentCount
                };

                _db.StudentGroupsCore.Add(entity);
            }
            else
            {
                entity.EducationDirectionId = direction.Id;
                entity.CuratorId = curator?.Id;
                entity.GroupName = dto.GroupName;
                entity.Course = dto.Course;
                entity.StudentCount = studentCount;
            }
        }

        await _db.SaveChangesAsync();
    }
}