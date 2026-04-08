using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.AcademicPlan;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Core;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
using DepartmentLoadApp.Models.Workload;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Data;

public class DepartmentLoadDbContext : DbContext
{
    public DepartmentLoadDbContext(DbContextOptions<DepartmentLoadDbContext> options)
        : base(options)
    {
    }

    // CORE
    public DbSet<EducationDirection> EducationDirections { get; set; }
    public DbSet<Lecturer> Lecturers { get; set; }
    public DbSet<StudentGroup> StudentGroupsCore { get; set; }
    public DbSet<AcademicPlan> AcademicPlansCore { get; set; }
    public DbSet<AcademicPlanRecord> AcademicPlanRecordsCore { get; set; }
    public DbSet<LecturerStudyPost> LecturerStudyPosts { get; set; }
    public DbSet<LecturerDepartmentPost> LecturerDepartmentPosts { get; set; }

    // ТВОЙ МОДУЛЬ
    public DbSet<ContingentRow> ContingentRows { get; set; }
    public DbSet<NormTime> NormTimes { get; set; }
    public DbSet<Discipline> Disciplines { get; set; }
    public DbSet<WorkloadRow> WorkloadRows { get; set; }
    public DbSet<LoadCalculation> LoadCalculations { get; set; }
    public DbSet<LoadDistribution> LoadDistributions { get; set; }
    public DbSet<PracticeWorkloadRow> PracticeWorkloadRows { get; set; }
    public DbSet<GiaWorkloadRow> GiaWorkloadRows { get; set; }
    public DbSet<SemesterPeriod> SemesterPeriods { get; set; }
    public DbSet<StudentFlow> StudentFlows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EducationDirection>()
            .HasIndex(x => x.CoreId)
            .IsUnique();

        modelBuilder.Entity<LecturerStudyPost>()
            .HasIndex(x => x.CoreId)
            .IsUnique();

        modelBuilder.Entity<LecturerDepartmentPost>()
            .HasIndex(x => x.CoreId)
            .IsUnique();

        modelBuilder.Entity<Lecturer>()
            .HasIndex(x => x.CoreId)
            .IsUnique();

        modelBuilder.Entity<StudentGroup>()
            .HasIndex(x => x.CoreId)
            .IsUnique();

        modelBuilder.Entity<AcademicPlan>()
            .HasIndex(x => x.CoreId)
            .IsUnique();

        modelBuilder.Entity<AcademicPlanRecord>()
            .HasIndex(x => x.CoreId)
            .IsUnique();

        modelBuilder.Entity<Lecturer>()
            .HasOne(l => l.StudyPost)
            .WithMany()
            .HasForeignKey(l => l.LecturerStudyPostId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lecturer>()
            .HasOne(l => l.DepartmentPost)
            .WithMany()
            .HasForeignKey(l => l.LecturerDepartmentPostId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentGroup>()
            .HasOne(g => g.EducationDirection)
            .WithMany()
            .HasForeignKey(g => g.EducationDirectionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentGroup>()
            .HasOne(g => g.Curator)
            .WithMany()
            .HasForeignKey(g => g.CuratorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AcademicPlan>()
            .HasOne(p => p.EducationDirection)
            .WithMany()
            .HasForeignKey(p => p.EducationDirectionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AcademicPlanRecord>()
            .HasOne(r => r.AcademicPlan)
            .WithMany(p => p.Records)
            .HasForeignKey(r => r.AcademicPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LoadDistribution>()
            .HasOne(ld => ld.Lecturer)
            .WithMany()
            .HasForeignKey(ld => ld.LecturerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LoadDistribution>()
            .HasOne(ld => ld.LoadCalculation)
            .WithMany()
            .HasForeignKey(ld => ld.LoadCalculationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SemesterPeriod>()
            .Property(x => x.AcademicYear)
            .HasMaxLength(9);

        modelBuilder.Entity<StudentFlow>()
            .Property(x => x.AcademicYear)
            .HasMaxLength(9);
    }
}