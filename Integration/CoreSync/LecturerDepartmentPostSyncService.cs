using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class LecturerDepartmentPostSyncService
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
        var items = await _api.GetListAsync<LecturerDepartmentPostDto>("LecturerDepartmentPosts/get-all");

        foreach (var dto in items)
        {
            var entity = await _db.Set<LecturerDepartmentPost>()
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            if (entity == null)
            {
                _db.Add(new LecturerDepartmentPost
                {
                    CoreId = dto.Id,
                    Title = dto.Title,
                    Order = dto.Order
                });
            }
            else
            {
                entity.Title = dto.Title;
                entity.Order = dto.Order;
            }
        }

        await _db.SaveChangesAsync();
    }
}