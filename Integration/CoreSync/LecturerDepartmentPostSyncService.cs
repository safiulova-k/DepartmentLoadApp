using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class LecturerDepartmentPostSyncService : ILecturerDepartmentPostSyncService
{
    private readonly CoreApiService _api;
    private readonly DepartmentLoadDbContext _db;

    public LecturerDepartmentPostSyncService(CoreApiService api, DepartmentLoadDbContext db)
    {
        _api = api;
        _db = db;
    }

    public async Task Sync()
    {
        var items = await _api.GetListAsync<LecturerDepartmentPostDto>("LecturerDepartmentPosts/GetLecturerDepartmentPostList");

        foreach (var dto in items)
        {
            var entity = await _db.LecturerDepartmentPosts
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            if (entity == null)
            {
                entity = new LecturerDepartmentPost
                {
                    CoreId = dto.Id,
                    DepartmentPostTitle = dto.DepartmentPostTitle,
                    Order = dto.Order
                };

                _db.LecturerDepartmentPosts.Add(entity);
            }
            else
            {
                entity.DepartmentPostTitle = dto.DepartmentPostTitle;
                entity.Order = dto.Order;
            }
        }

        await _db.SaveChangesAsync();
    }
}