using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class LecturerStudyPostSyncService
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
        var items = await _api.GetListAsync<LecturerStudyPostDto>("LecturerStudyPosts/get-all");

        foreach (var dto in items)
        {
            var entity = await _db.Set<LecturerStudyPost>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            if (entity == null)
            {
                _db.Add(new LecturerStudyPost
                {
                    CoreId = dto.Id,
                    Title = dto.Title,
                    Hours = dto.Hours
                });
            }
            else
            {
                entity.Title = dto.Title;
                entity.Hours = dto.Hours;
            }
        }

        await _db.SaveChangesAsync();
    }
}