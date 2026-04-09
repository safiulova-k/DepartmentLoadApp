using DepartmentLoadApp.Models;
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

    // Core
    public DbSet<EducationDirection> EducationDirections { get; set; } = null!;
    public DbSet<Lecturer> Lecturers { get; set; } = null!;
    public DbSet<StudentGroup> StudentGroupsCore { get; set; } = null!;
    public DbSet<AcademicPlan> AcademicPlansCore { get; set; } = null!;
    public DbSet<AcademicPlanRecord> AcademicPlanRecordsCore { get; set; } = null!;
    public DbSet<LecturerStudyPost> LecturerStudyPosts { get; set; } = null!;
    public DbSet<LecturerDepartmentPost> LecturerDepartmentPosts { get; set; } = null!;

    // Твой модуль
    public DbSet<ContingentRow> ContingentRows { get; set; } = null!;
    public DbSet<NormTime> NormTimes { get; set; } = null!;
    public DbSet<WorkloadRow> WorkloadRows { get; set; } = null!;
    public DbSet<LoadCalculation> LoadCalculations { get; set; } = null!;
    public DbSet<LoadDistribution> LoadDistributions { get; set; } = null!;
    public DbSet<PracticeWorkloadRow> PracticeWorkloadRows { get; set; } = null!;
    public DbSet<GiaWorkloadRow> GiaWorkloadRows { get; set; } = null!;
    public DbSet<SemesterPeriod> SemesterPeriods { get; set; } = null!;
    public DbSet<StudentFlow> StudentFlows { get; set; } = null!;

    // Распределение нагрузки
    public DbSet<LecturerAcademicYearPlan> LecturerAcademicYearPlans { get; set; } = null!;
    public DbSet<LecturerLoadAssignment> LecturerLoadAssignments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- CORE ----------
        modelBuilder.Entity<EducationDirection>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CoreId).IsUnique();

            entity.Property(x => x.Cipher).IsRequired();
            entity.Property(x => x.ShortName).IsRequired();
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.Profile).IsRequired();
            entity.Property(x => x.Description).IsRequired();
        });

        modelBuilder.Entity<LecturerStudyPost>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CoreId).IsUnique();
            entity.Property(x => x.StudyPostTitle).IsRequired();
        });

        modelBuilder.Entity<LecturerDepartmentPost>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CoreId).IsUnique();
            entity.Property(x => x.DepartmentPostTitle).IsRequired();
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CoreId).IsUnique();

            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.LastName).IsRequired();
            entity.Property(x => x.Patronymic).IsRequired();
            entity.Property(x => x.Abbreviation).IsRequired();
            entity.Property(x => x.Address).IsRequired();
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.MobileNumber).IsRequired();
            entity.Property(x => x.HomeNumber).IsRequired();
            entity.Property(x => x.Description).IsRequired();

            entity.HasOne<LecturerStudyPost>()
                .WithMany()
                .HasForeignKey(x => x.LecturerStudyPostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<LecturerDepartmentPost>()
                .WithMany()
                .HasForeignKey(x => x.LecturerDepartmentPostId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentGroup>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CoreId).IsUnique();
            entity.Property(x => x.GroupName).IsRequired();

            entity.HasOne<EducationDirection>()
                .WithMany()
                .HasForeignKey(x => x.EducationDirectionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Lecturer>()
                .WithMany()
                .HasForeignKey(x => x.CuratorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AcademicPlan>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CoreId).IsUnique();
            entity.Property(x => x.Year).IsRequired();

            entity.HasOne<EducationDirection>()
                .WithMany()
                .HasForeignKey(x => x.EducationDirectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AcademicPlanRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CoreId).IsUnique();
            entity.Property(x => x.Index).IsRequired();
            entity.Property(x => x.Name).IsRequired();

            entity.HasOne<AcademicPlan>()
                .WithMany()
                .HasForeignKey(x => x.AcademicPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- ТВОЙ МОДУЛЬ ----------
        modelBuilder.Entity<LoadDistribution>(entity =>
        {
            entity.HasOne(x => x.Lecturer)
                .WithMany()
                .HasForeignKey(x => x.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LoadCalculation)
                .WithMany()
                .HasForeignKey(x => x.LoadCalculationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SemesterPeriod>()
            .Property(x => x.AcademicYear)
            .HasMaxLength(9);

        modelBuilder.Entity<StudentFlow>()
            .Property(x => x.AcademicYear)
            .HasMaxLength(9);

        // ---------- РАСПРЕДЕЛЕНИЕ НАГРУЗКИ ----------
        modelBuilder.Entity<LecturerAcademicYearPlan>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AcademicYear)
                .IsRequired()
                .HasMaxLength(9);

            entity.Property(x => x.Rate)
                .HasPrecision(5, 2);

            entity.HasIndex(x => new { x.AcademicYear, x.LecturerId })
                .IsUnique();

            entity.HasOne(x => x.Lecturer)
                .WithMany()
                .HasForeignKey(x => x.LecturerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.LecturerStudyPost)
                .WithMany()
                .HasForeignKey(x => x.LecturerStudyPostId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LecturerLoadAssignment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AcademicYear)
                .IsRequired()
                .HasMaxLength(9);

            entity.Property(x => x.SourceType)
                .HasConversion<string>();

            entity.Property(x => x.LoadElementType)
                .HasConversion<string>();

            entity.Property(x => x.AssignedHours)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.LecturerAcademicYearPlanId,
                x.SourceType,
                x.SourceRowId,
                x.LoadElementType
            }).IsUnique();

            entity.HasIndex(x => new
            {
                x.AcademicYear,
                x.SourceType,
                x.SourceRowId,
                x.LoadElementType
            });

            entity.HasOne(x => x.LecturerAcademicYearPlan)
                .WithMany()
                .HasForeignKey(x => x.LecturerAcademicYearPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}