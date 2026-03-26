using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.NormTime;
using DepartmentLoadApp.Models.Workload;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Data
{
    public class DepartmentLoadDbContext : DbContext
    {
        public DepartmentLoadDbContext(DbContextOptions<DepartmentLoadDbContext> options)
            : base(options)
        {
        }

        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<ContingentRow> ContingentRows => Set<ContingentRow>();
        public DbSet<NormTime> NormTimes => Set<NormTime>();
        public DbSet<WorkloadRow> WorkloadRows => Set<WorkloadRow>();
        public DbSet<LoadCalculation> LoadCalculations => Set<LoadCalculation>();
        public DbSet<LoadDistribution> LoadDistributions => Set<LoadDistribution>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teachers");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Position)
                    .HasMaxLength(100);

                entity.Property(x => x.Degree)
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ContingentRow>(entity =>
            {
                entity.ToTable("ContingentRows");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DirectionCode)
                    .IsRequired()
                    .HasMaxLength(50);

            });

            modelBuilder.Entity<NormTime>(entity =>
            {
                entity.ToTable("NormTimes");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.WorkTypeName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.UnitName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.HoursValue)
                    .HasColumnType("numeric(10,2)");

                entity.Property(x => x.Note)
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<WorkloadRow>(entity =>
            {
                entity.ToTable("WorkloadRows");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DirectionCode)
                    .HasMaxLength(50);

                entity.Property(x => x.DirectionName)
                    .HasMaxLength(200);

                entity.Property(x => x.SemesterName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.EducationForm)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.LecturePlanHours)
                    .HasColumnType("numeric(10,2)");

                entity.Property(x => x.LectureTotalHours)
                    .HasColumnType("numeric(10,2)");

                entity.Property(x => x.PracticePlanHours)
                    .HasColumnType("numeric(10,2)");

                entity.Property(x => x.PracticeTotalHours)
                    .HasColumnType("numeric(10,2)");

                entity.Property(x => x.LabPlanHours)
                    .HasColumnType("numeric(10,2)");

                entity.Property(x => x.LabTotalHours)
                    .HasColumnType("numeric(10,2)");
            });

            modelBuilder.Entity<LoadCalculation>(entity =>
            {
                entity.ToTable("LoadCalculations");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.LoadType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.TotalHours)
                    .HasColumnType("numeric(10,2)");
            });

            modelBuilder.Entity<LoadDistribution>(entity =>
            {
                entity.ToTable("LoadDistributions");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Hours)
                    .HasColumnType("numeric(10,2)");

                entity.HasOne(x => x.Teacher)
                    .WithMany(x => x.LoadDistributions)
                    .HasForeignKey(x => x.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.LoadCalculation)
                    .WithMany(x => x.LoadDistributions)
                    .HasForeignKey(x => x.LoadCalculationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}