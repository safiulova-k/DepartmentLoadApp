using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Core.Enums;
using DepartmentLoadApp.ViewModels.Contingent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers;

public class ContingentController : Controller
{
    private readonly DepartmentLoadDbContext _context;

    public ContingentController(DepartmentLoadDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await SyncContingentAsync();
        var model = await BuildPageModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubgroup(int studentGroupId)
    {
        var group = await _context.StudentGroupsCore
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == studentGroupId);

        if (group == null)
        {
            TempData["ErrorMessage"] = "Группа не найдена";
            return RedirectToAction(nameof(Index));
        }

        await EnsureContingentSubgroupsAsync();

        var nextNumber = await _context.ContingentSubgroups
            .Where(x => x.StudentGroupId == studentGroupId)
            .CountAsync() + 1;

        _context.ContingentSubgroups.Add(new ContingentSubgroup
        {
            StudentGroupId = studentGroupId,
            SubgroupNumber = nextNumber,
            StudentsCount = 0
        });

        await _context.SaveChangesAsync();
        await DistributeStudentsEvenlyAsync(studentGroupId);
        await RebuildContingentRowsAsync();

        TempData["SuccessMessage"] = "Подгруппа добавлена";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubgroup(int studentGroupId, int subgroupId)
    {
        await EnsureContingentSubgroupsAsync();

        var subgroups = await _context.ContingentSubgroups
            .Where(x => x.StudentGroupId == studentGroupId)
            .OrderBy(x => x.SubgroupNumber)
            .ToListAsync();

        if (subgroups.Count <= 1)
        {
            TempData["ErrorMessage"] = "У группы должна остаться хотя бы одна подгруппа";
            return RedirectToAction(nameof(Index));
        }

        var subgroup = subgroups.FirstOrDefault(x => x.Id == subgroupId);
        if (subgroup == null)
        {
            TempData["ErrorMessage"] = "Подгруппа не найдена";
            return RedirectToAction(nameof(Index));
        }

        _context.ContingentSubgroups.Remove(subgroup);
        await _context.SaveChangesAsync();

        await RenumberSubgroupsAsync(studentGroupId);
        await DistributeStudentsEvenlyAsync(studentGroupId);
        await RebuildContingentRowsAsync();

        TempData["SuccessMessage"] = "Подгруппа удалена";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGroupSubgroups(
        int studentGroupId,
        List<int> subgroupIds,
        List<int> studentsCounts)
    {
        if (subgroupIds.Count != studentsCounts.Count)
        {
            TempData["ErrorMessage"] = "Не удалось сохранить подгруппы";
            return RedirectToAction(nameof(Index));
        }

        var group = await _context.StudentGroupsCore
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == studentGroupId);

        if (group == null)
        {
            TempData["ErrorMessage"] = "Группа не найдена";
            return RedirectToAction(nameof(Index));
        }

        var normalizedCounts = studentsCounts
            .Select(x => x < 0 ? 0 : x)
            .ToList();

        if (normalizedCounts.Sum() != group.StudentCount)
        {
            TempData["ErrorMessage"] =
                $"Сумма студентов по подгруппам должна быть равна {group.StudentCount}";
            return RedirectToAction(nameof(Index));
        }

        var subgroups = await _context.ContingentSubgroups
            .Where(x => x.StudentGroupId == studentGroupId && subgroupIds.Contains(x.Id))
            .OrderBy(x => x.SubgroupNumber)
            .ToListAsync();

        if (subgroups.Count != subgroupIds.Count)
        {
            TempData["ErrorMessage"] = "Некоторые подгруппы не найдены";
            return RedirectToAction(nameof(Index));
        }

        var countsById = subgroupIds
            .Select((id, index) => new { id, count = normalizedCounts[index] })
            .ToDictionary(x => x.id, x => x.count);

        foreach (var subgroup in subgroups)
        {
            subgroup.StudentsCount = countsById[subgroup.Id];
        }

        await _context.SaveChangesAsync();
        await RenumberSubgroupsAsync(studentGroupId);
        await RebuildContingentRowsAsync();

        TempData["SuccessMessage"] = "Подгруппы сохранены";
        return RedirectToAction(nameof(Index));
    }

    private async Task SyncContingentAsync()
    {
        await EnsureContingentSubgroupsAsync();
        await RebuildContingentRowsAsync();
    }

    private async Task EnsureContingentSubgroupsAsync()
    {
        var studentGroups = await _context.StudentGroupsCore
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.StudentCount
            })
            .ToListAsync();

        var studentGroupIds = studentGroups
            .Select(x => x.Id)
            .ToHashSet();

        var existingSubgroups = await _context.ContingentSubgroups
            .OrderBy(x => x.StudentGroupId)
            .ThenBy(x => x.SubgroupNumber)
            .ToListAsync();

        var orphanedSubgroups = existingSubgroups
            .Where(x => !studentGroupIds.Contains(x.StudentGroupId))
            .ToList();

        if (orphanedSubgroups.Any())
        {
            _context.ContingentSubgroups.RemoveRange(orphanedSubgroups);
        }

        foreach (var studentGroup in studentGroups)
        {
            var groupSubgroups = existingSubgroups
                .Where(x => x.StudentGroupId == studentGroup.Id)
                .OrderBy(x => x.SubgroupNumber)
                .ToList();

            if (!groupSubgroups.Any())
            {
                _context.ContingentSubgroups.Add(new ContingentSubgroup
                {
                    StudentGroupId = studentGroup.Id,
                    SubgroupNumber = 1,
                    StudentsCount = studentGroup.StudentCount
                });

                continue;
            }

            var changed = false;

            for (var i = 0; i < groupSubgroups.Count; i++)
            {
                var requiredNumber = i + 1;
                if (groupSubgroups[i].SubgroupNumber != requiredNumber)
                {
                    groupSubgroups[i].SubgroupNumber = requiredNumber;
                    changed = true;
                }
            }

            if (groupSubgroups.Sum(x => x.StudentsCount) != studentGroup.StudentCount)
            {
                ApplyEvenDistribution(groupSubgroups, studentGroup.StudentCount);
                changed = true;
            }

            if (changed)
            {
                _context.ContingentSubgroups.UpdateRange(groupSubgroups);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task DistributeStudentsEvenlyAsync(int studentGroupId)
    {
        var studentGroup = await _context.StudentGroupsCore
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == studentGroupId);

        if (studentGroup == null)
        {
            return;
        }

        var subgroups = await _context.ContingentSubgroups
            .Where(x => x.StudentGroupId == studentGroupId)
            .OrderBy(x => x.SubgroupNumber)
            .ToListAsync();

        if (!subgroups.Any())
        {
            return;
        }

        ApplyEvenDistribution(subgroups, studentGroup.StudentCount);

        _context.ContingentSubgroups.UpdateRange(subgroups);
        await _context.SaveChangesAsync();
    }

    private async Task RenumberSubgroupsAsync(int studentGroupId)
    {
        var subgroups = await _context.ContingentSubgroups
            .Where(x => x.StudentGroupId == studentGroupId)
            .OrderBy(x => x.SubgroupNumber)
            .ToListAsync();

        for (var i = 0; i < subgroups.Count; i++)
        {
            subgroups[i].SubgroupNumber = i + 1;
        }

        await _context.SaveChangesAsync();
    }

    private static void ApplyEvenDistribution(List<ContingentSubgroup> subgroups, int totalStudents)
    {
        if (!subgroups.Any())
        {
            return;
        }

        var safeTotal = totalStudents < 0 ? 0 : totalStudents;
        var baseCount = safeTotal / subgroups.Count;
        var remainder = safeTotal % subgroups.Count;

        for (var i = 0; i < subgroups.Count; i++)
        {
            subgroups[i].StudentsCount = baseCount + (i < remainder ? 1 : 0);
        }
    }

    private async Task RebuildContingentRowsAsync()
    {
        var subgroupCounts = await _context.ContingentSubgroups
             .AsNoTracking()
             .GroupBy(x => x.StudentGroupId)
             .Select(g => new
             {
                 StudentGroupId = g.Key,
                 Count = g.Count()
             })
             .ToDictionaryAsync(x => x.StudentGroupId, x => x.Count);

        var groups = await (
            from studentGroup in _context.StudentGroupsCore.AsNoTracking()
            join direction in _context.EducationDirections.AsNoTracking()
                on studentGroup.EducationDirectionId equals direction.Id
            select new
            {
                DirectionCode = direction.Cipher,
                direction.Qualification,
                studentGroup.Id,
                studentGroup.Course,
                studentGroup.StudentCount
            })
            .ToListAsync();

        var newRows = groups
            .GroupBy(x => new
            {
                x.DirectionCode,
                x.Qualification
            })
            .OrderBy(x => x.Key.DirectionCode)
            .ThenBy(x => x.Key.Qualification)
            .Select(x => new ContingentRow
            {
                DirectionCode = x.Key.DirectionCode,
                IsBachelor = x.Key.Qualification == EducationDirectionQualification.Бакалавриат,
                IsMaster = x.Key.Qualification == EducationDirectionQualification.Магистратура,

                Course1Count = x.Where(g => g.Course == AcademicCourse.Course_1).Sum(g => g.StudentCount),
                Course2Count = x.Where(g => g.Course == AcademicCourse.Course_2).Sum(g => g.StudentCount),
                Course3Count = x.Where(g => g.Course == AcademicCourse.Course_3).Sum(g => g.StudentCount),
                Course4Count = x.Where(g => g.Course == AcademicCourse.Course_4).Sum(g => g.StudentCount),

                Course1Groups = x.Count(g => g.Course == AcademicCourse.Course_1),
                Course2Groups = x.Count(g => g.Course == AcademicCourse.Course_2),
                Course3Groups = x.Count(g => g.Course == AcademicCourse.Course_3),
                Course4Groups = x.Count(g => g.Course == AcademicCourse.Course_4),

                Course1Subgroups = x.Where(g => g.Course == AcademicCourse.Course_1)
                    .Sum(g => subgroupCounts.TryGetValue(g.Id, out var count) ? count : 1),
                Course2Subgroups = x.Where(g => g.Course == AcademicCourse.Course_2)
                    .Sum(g => subgroupCounts.TryGetValue(g.Id, out var count) ? count : 1),
                Course3Subgroups = x.Where(g => g.Course == AcademicCourse.Course_3)
                    .Sum(g => subgroupCounts.TryGetValue(g.Id, out var count) ? count : 1),
                Course4Subgroups = x.Where(g => g.Course == AcademicCourse.Course_4)
                    .Sum(g => subgroupCounts.TryGetValue(g.Id, out var count) ? count : 1),

                TotalCount = x.Sum(g => g.StudentCount)
            })
            .ToList();

        var oldRows = await _context.ContingentRows.ToListAsync();
        if (oldRows.Any())
        {
            _context.ContingentRows.RemoveRange(oldRows);
            await _context.SaveChangesAsync();
        }

        if (newRows.Any())
        {
            await _context.ContingentRows.AddRangeAsync(newRows);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<ContingentPageViewModel> BuildPageModelAsync()
    {
        var rows = await _context.ContingentRows
            .AsNoTracking()
            .OrderBy(x => x.DirectionCode)
            .ThenByDescending(x => x.IsBachelor)
            .ToListAsync();

        var subgroups = await _context.ContingentSubgroups
            .AsNoTracking()
            .OrderBy(x => x.StudentGroupId)
            .ThenBy(x => x.SubgroupNumber)
            .ToListAsync();

        var subgroupsByGroupId = subgroups
            .GroupBy(x => x.StudentGroupId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var groupItems = await (
            from studentGroup in _context.StudentGroupsCore.AsNoTracking()
            join direction in _context.EducationDirections.AsNoTracking()
                on studentGroup.EducationDirectionId equals direction.Id
            orderby direction.Cipher, direction.Qualification, studentGroup.Course, studentGroup.GroupName
            select new
            {
                studentGroup.Id,
                studentGroup.GroupName,
                studentGroup.StudentCount,
                Course = (int)studentGroup.Course,
                DirectionCode = direction.Cipher,
                direction.Qualification
            })
            .ToListAsync();

        var directions = groupItems
            .GroupBy(x => new
            {
                x.DirectionCode,
                x.Qualification
            })
            .OrderBy(x => x.Key.DirectionCode)
            .ThenBy(x => x.Key.Qualification)
            .Select(x => new ContingentDirectionViewModel
            {
                DirectionCode = x.Key.DirectionCode,
                IsBachelor = x.Key.Qualification == EducationDirectionQualification.Бакалавриат,
                QualificationName = x.Key.Qualification == EducationDirectionQualification.Бакалавриат
                    ? "Бакалавриат"
                    : "Магистратура",
                Courses = Enumerable.Range(1, 4)
                    .Select(courseNumber => new ContingentCourseViewModel
                    {
                        CourseNumber = courseNumber,
                        Groups = x.Where(g => g.Course == courseNumber)
                            .Select(g => new ContingentGroupViewModel
                            {
                                StudentGroupId = g.Id,
                                GroupName = g.GroupName,
                                StudentsCount = g.StudentCount,
                                Subgroups = subgroupsByGroupId.TryGetValue(g.Id, out var groupSubgroups)
                                    ? groupSubgroups.Select(s => new ContingentSubgroupViewModel
                                    {
                                        Id = s.Id,
                                        StudentGroupId = s.StudentGroupId,
                                        SubgroupNumber = s.SubgroupNumber,
                                        StudentsCount = s.StudentsCount
                                    }).ToList()
                                    : new List<ContingentSubgroupViewModel>
                                    {
                                        new ContingentSubgroupViewModel
                                        {
                                            Id = 0,
                                            StudentGroupId = g.Id,
                                            SubgroupNumber = 1,
                                            StudentsCount = g.StudentCount
                                        }
                                    }
                            })
                            .ToList()
                    })
                    .Where(c => c.Groups.Any())
                    .ToList()
            })
            .ToList();

        return new ContingentPageViewModel
        {
            Rows = rows,
            Directions = directions
        };
    }
}