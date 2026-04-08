using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class LecturerSyncService : ILecturerSyncService
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
        var items = await _api.GetListAsync<LecturerDto>("Lecturers/GetLecturerList");

        foreach (var dto in items)
        {
            var entity = await _db.Lecturers
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            LecturerStudyPost? studyPost = null;
            if (dto.LecturerStudyPostId.HasValue)
            {
                studyPost = await _db.LecturerStudyPosts
                    .FirstOrDefaultAsync(x => x.CoreId == dto.LecturerStudyPostId.Value);
            }

            var departmentPost = await _db.LecturerDepartmentPosts
                .FirstOrDefaultAsync(x => x.CoreId == dto.LecturerDepartmentPostId);

            if (departmentPost == null)
            {
                continue;
            }

            if (entity == null)
            {
                entity = new Lecturer
                {
                    CoreId = dto.Id,
                    LecturerStudyPostId = studyPost?.Id,
                    LecturerDepartmentPostId = departmentPost.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Patronymic = dto.Patronymic,
                    Abbreviation = dto.Abbreviation,
                    DateBirth = dto.DateBirth,
                    Address = dto.Address,
                    Email = dto.Email,
                    MobileNumber = dto.MobileNumber,
                    HomeNumber = dto.HomeNumber,
                    Rank = dto.Rank,
                    Rank2 = dto.Rank2,
                    Description = dto.Description,
                    Photo = dto.Photo,
                    OnlyForPrivate = dto.OnlyForPrivate
                };

                _db.Lecturers.Add(entity);
            }
            else
            {
                entity.LecturerStudyPostId = studyPost?.Id;
                entity.LecturerDepartmentPostId = departmentPost.Id;
                entity.FirstName = dto.FirstName;
                entity.LastName = dto.LastName;
                entity.Patronymic = dto.Patronymic;
                entity.Abbreviation = dto.Abbreviation;
                entity.DateBirth = dto.DateBirth;
                entity.Address = dto.Address;
                entity.Email = dto.Email;
                entity.MobileNumber = dto.MobileNumber;
                entity.HomeNumber = dto.HomeNumber;
                entity.Rank = dto.Rank;
                entity.Rank2 = dto.Rank2;
                entity.Description = dto.Description;
                entity.Photo = dto.Photo;
                entity.OnlyForPrivate = dto.OnlyForPrivate;
            }
        }

        await _db.SaveChangesAsync();
    }
}