using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class EducationDirectionSyncService : IEducationDirectionSyncService
{
    private readonly CoreApiService _api;
    private readonly DepartmentLoadDbContext _db;

    public EducationDirectionSyncService(CoreApiService api, DepartmentLoadDbContext db)
    {
        _api = api;
        _db = db;
    }

    public async Task Sync()
    {
        var items = await _api.GetListAsync<EducationDirectionDto>("EducationDirections/GetEducationDirectionList");

        foreach (var dto in items)
        {
            var entity = await _db.EducationDirections
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            if (entity == null)
            {
                entity = new EducationDirection
                {
                    CoreId = dto.Id,
                    Cipher = dto.Cipher,
                    ShortName = dto.ShortName,
                    Title = dto.Title,
                    Qualification = dto.Qualification,
                    Profile = dto.Profile,
                    Description = dto.Description
                };

                _db.EducationDirections.Add(entity);
            }
            else
            {
                entity.Cipher = dto.Cipher;
                entity.ShortName = dto.ShortName;
                entity.Title = dto.Title;
                entity.Qualification = dto.Qualification;
                entity.Profile = dto.Profile;
                entity.Description = dto.Description;
            }
        }

        await _db.SaveChangesAsync();
    }
}