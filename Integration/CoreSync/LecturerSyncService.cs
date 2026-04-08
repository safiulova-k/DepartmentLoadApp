using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class LecturerSyncService
{
    private readonly CoreApiService _api;
    private readonly DepartmentLoadDbContext _db;

    public LecturerSyncService(CoreApiService api, DepartmentLoadDbContext db)
    {
        _api = api;
        _db = db;
    }

    public async Task Sync()
    {
        var items = await _api.GetListAsync<LecturerDto>("Lecturers/get-all");

        foreach (var dto in items)
        {
            var entity = await _db.Set<Lecturer>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            var studyPost = await _db.Set<LecturerStudyPost>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.LecturerStudyPostId);

            var depPost = await _db.Set<LecturerDepartmentPost>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.LecturerDepartmentPostId);

            if (entity == null)
            {
                _db.Add(new Lecturer
                {
                    CoreId = dto.Id,
                    LastName = dto.LastName,
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName,
                    LecturerStudyPostId = studyPost!.Id,
                    LecturerDepartmentPostId = depPost!.Id
                });
            }
            else
            {
                entity.LastName = dto.LastName;
                entity.FirstName = dto.FirstName;
                entity.MiddleName = dto.MiddleName;
                entity.LecturerStudyPostId = studyPost!.Id;
                entity.LecturerDepartmentPostId = depPost!.Id;
            }
        }

        await _db.SaveChangesAsync();
    }
}