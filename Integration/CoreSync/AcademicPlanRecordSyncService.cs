using DepartmentLoadApp.Data;
using DepartmentLoadApp.Dtos.Core;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Integration.CoreSync;

public class AcademicPlanRecordSyncService : IAcademicPlanRecordSyncService
{
    private readonly CoreApiService _api;
    private readonly DepartmentLoadDbContext _db;

    public AcademicPlanRecordSyncService(CoreApiService api, DepartmentLoadDbContext db)
    {
        _api = api;
        _db = db;
    }

    public async Task Sync()
    {
        var items = await _api.GetListAsync<AcademicPlanRecordDto>("AcademicPlanRecords/GetAcademicPlanRecordList");

        foreach (var dto in items)
        {
            var entity = await _db.AcademicPlanRecordsCore
                .FirstOrDefaultAsync(x => x.CoreId == dto.Id);

            var academicPlan = await _db.AcademicPlansCore
                .FirstOrDefaultAsync(x => x.CoreId == dto.AcademicPlanId);

            if (academicPlan == null)
            {
                continue;
            }

            if (entity == null)
            {
                entity = new AcademicPlanRecord
                {
                    CoreId = dto.Id,
                    AcademicPlanId = academicPlan.Id,
                    Index = dto.Index,
                    Name = dto.Name,
                    Semester = dto.Semester,
                    Zet = dto.Zet,
                    AcademicHours = dto.AcademicHours,
                    Exam = dto.Exam,
                    Pass = dto.Pass,
                    GradedPass = dto.GradedPass,
                    CourseWork = dto.CourseWork,
                    CourseProject = dto.CourseProject,
                    Rgr = dto.Rgr,
                    Lectures = dto.Lectures,
                    LaboratoryHours = dto.LaboratoryHours,
                    PracticalHours = dto.PracticalHours
                };

                _db.AcademicPlanRecordsCore.Add(entity);
            }
            else
            {
                entity.AcademicPlanId = academicPlan.Id;
                entity.Index = dto.Index;
                entity.Name = dto.Name;
                entity.Semester = dto.Semester;
                entity.Zet = dto.Zet;
                entity.AcademicHours = dto.AcademicHours;
                entity.Exam = dto.Exam;
                entity.Pass = dto.Pass;
                entity.GradedPass = dto.GradedPass;
                entity.CourseWork = dto.CourseWork;
                entity.CourseProject = dto.CourseProject;
                entity.Rgr = dto.Rgr;
                entity.Lectures = dto.Lectures;
                entity.LaboratoryHours = dto.LaboratoryHours;
                entity.PracticalHours = dto.PracticalHours;
            }
        }

        await _db.SaveChangesAsync();
    }
}