using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class LecturerSyncService : ILecturerSyncService
{
    private readonly CoreApiService _api;
    private readonly DepartmentLoadDbContext _db;

    public LecturerSyncService(
        CoreApiService api,
        DepartmentLoadDbContext db)
    {
        _api = api;
        _db = db;
    }

    public async Task Sync()
    {
        var items = await _api.GetListAsync<LecturerDto>("Lecturers/GetLecturerList");

        await using var transaction = await _db.Database.BeginTransactionAsync();

        await DeleteMissingLecturersAsync(items);
        await UpsertLecturersAsync(items);

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task DeleteMissingLecturersAsync(List<LecturerDto> coreItems)
    {
        var actualCoreIds = coreItems
            .Select(x => x.Id)
            .ToHashSet();

        var lecturersToDelete = await _db.Lecturers
            .Where(x => !actualCoreIds.Contains(x.CoreId))
            .ToListAsync();

        if (lecturersToDelete.Count == 0)
        {
            return;
        }

        var lecturerIdsToDelete = lecturersToDelete
            .Select(x => x.Id)
            .ToList();

        var studentGroupsWithDeletedCurator = await _db.StudentGroupsCore
            .Where(x =>
                x.CuratorId.HasValue &&
                lecturerIdsToDelete.Contains(x.CuratorId.Value))
            .ToListAsync();

        foreach (var group in studentGroupsWithDeletedCurator)
        {
            group.CuratorId = null;
        }

        var oldLoadDistributions = await _db.LoadDistributions
            .Where(x => lecturerIdsToDelete.Contains(x.LecturerId))
            .ToListAsync();

        if (oldLoadDistributions.Count > 0)
        {
            _db.LoadDistributions.RemoveRange(oldLoadDistributions);
        }

        var lecturerPlansToDelete = await _db.LecturerAcademicYearPlans
            .Where(x => lecturerIdsToDelete.Contains(x.LecturerId))
            .ToListAsync();

        if (lecturerPlansToDelete.Count > 0)
        {
            var lecturerPlanIdsToDelete = lecturerPlansToDelete
                .Select(x => x.Id)
                .ToList();

            var assignmentsToDelete = await _db.LecturerLoadAssignments
                .Where(x => lecturerPlanIdsToDelete.Contains(x.LecturerAcademicYearPlanId))
                .ToListAsync();

            if (assignmentsToDelete.Count > 0)
            {
                _db.LecturerLoadAssignments.RemoveRange(assignmentsToDelete);
            }

            _db.LecturerAcademicYearPlans.RemoveRange(lecturerPlansToDelete);
        }

        _db.Lecturers.RemoveRange(lecturersToDelete);
    }

    private async Task UpsertLecturersAsync(List<LecturerDto> items)
    {
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
                    CoreId = dto.Id
                };

                _db.Lecturers.Add(entity);
            }

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
}