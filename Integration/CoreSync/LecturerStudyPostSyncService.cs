using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class LecturerStudyPostSyncService : ILecturerStudyPostSyncService
{
    private readonly CoreApiService _api;
    private readonly DepartmentLoadDbContext _db;

    public LecturerStudyPostSyncService(CoreApiService api, DepartmentLoadDbContext db)
    {
        _api = api;
        _db = db;
    }

    public async Task Sync()
    {
        var items = await _api.GetListAsync<LecturerStudyPostDto>("LecturerStudyPosts/GetLecturerStudyPostList");

        foreach (var dto in items)
        {
            var entity = await _db.LecturerStudyPosts
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            if (entity == null)
            {
                entity = new LecturerStudyPost
                {
                    CoreId = dto.Id,
                    StudyPostTitle = dto.StudyPostTitle,
                    Hours = dto.Hours
                };

                _db.LecturerStudyPosts.Add(entity);
            }
            else
            {
                entity.StudyPostTitle = dto.StudyPostTitle;
                entity.Hours = dto.Hours;
            }
        }

        await _db.SaveChangesAsync();
    }
}