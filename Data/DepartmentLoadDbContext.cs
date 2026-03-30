using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.AcademicPlan;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.Models.Workload;
using DepartmentLoadApp.Models.Practice;
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

        public DbSet<AcademicPlan> AcademicPlans => Set<AcademicPlan>();
        public DbSet<AcademicPlanRecord> AcademicPlanRecords => Set<AcademicPlanRecord>();
        public DbSet<Discipline> Disciplines => Set<Discipline>();
        public DbSet<AcademicPlanRecordElement> AcademicPlanRecordElements => Set<AcademicPlanRecordElement>();

        public DbSet<WorkloadRow> WorkloadRows => Set<WorkloadRow>();
        public DbSet<LoadCalculation> LoadCalculations => Set<LoadCalculation>();
        public DbSet<LoadDistribution> LoadDistributions => Set<LoadDistribution>();
        public DbSet<PracticeWorkloadRow> PracticeWorkloadRows => Set<PracticeWorkloadRow>();

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

                entity.Property(x => x.WorkName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.CalculationBase)
                    .IsRequired();

                entity.Property(x => x.Hours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.Property(x => x.SortOrder)
                    .IsRequired();

                entity.HasData(
                    new NormTime { Id = 1, WorkName = "Лекции", CategoryName = "Аудиторная нагрузка", CalculationBase = WorkCalculationBase.PerStream, Hours = 1m, SortOrder = 1 },
                    new NormTime { Id = 2, WorkName = "Практические занятия", CategoryName = "Аудиторная нагрузка", CalculationBase = WorkCalculationBase.PerGroup, Hours = 1m, SortOrder = 2 },
                    new NormTime { Id = 3, WorkName = "Лабораторные работы", CategoryName = "Аудиторная нагрузка", CalculationBase = WorkCalculationBase.PerSubgroup, Hours = 1m, SortOrder = 3 },

                    new NormTime { Id = 4, WorkName = "Экзамены", CategoryName = "Контроль", CalculationBase = WorkCalculationBase.PerStudent, Hours = 0.50m, SortOrder = 4 },
                    new NormTime { Id = 5, WorkName = "Зачеты", CategoryName = "Контроль", CalculationBase = WorkCalculationBase.PerStudent, Hours = 0.25m, SortOrder = 5 },
                    new NormTime { Id = 6, WorkName = "Курсовая работа", CategoryName = "Контроль", CalculationBase = WorkCalculationBase.PerWork, Hours = 1m, SortOrder = 6 },
                    new NormTime { Id = 7, WorkName = "Курсовой проект", CategoryName = "Контроль", CalculationBase = WorkCalculationBase.PerWork, Hours = 1m, SortOrder = 7 },
                    new NormTime { Id = 8, WorkName = "Рефераты и РГР", CategoryName = "Контроль", CalculationBase = WorkCalculationBase.PerWork, Hours = 1m, SortOrder = 8 },
                    new NormTime { Id = 9, WorkName = "Консультации", CategoryName = "Контроль", CalculationBase = WorkCalculationBase.PerGroup, Hours = 1m, SortOrder = 9 },
                    new NormTime { Id = 10, WorkName = "Консультации перед экзаменом", CategoryName = "Контроль", CalculationBase = WorkCalculationBase.FromLectureHoursTotal, Hours = 1m, SortOrder = 10 },

                    new NormTime { Id = 11, WorkName = "Руководство ВКР бакалавра", CategoryName = "ВКР", CalculationBase = WorkCalculationBase.PerWork, Hours = 10m, SortOrder = 11 },
                    new NormTime { Id = 12, WorkName = "Руководство ВКР магистра", CategoryName = "ВКР", CalculationBase = WorkCalculationBase.PerWork, Hours = 15m, SortOrder = 12 },
                    new NormTime { Id = 13, WorkName = "Нормоконтроль ВКР", CategoryName = "ВКР", CalculationBase = WorkCalculationBase.PerWork, Hours = 1m, SortOrder = 13 },
                    new NormTime { Id = 14, WorkName = "ГосЭкзамен", CategoryName = "ВКР", CalculationBase = WorkCalculationBase.PerWork, Hours = 1m, SortOrder = 14 },

                    new NormTime { Id = 15, WorkName = "Учебная практика", CategoryName = "Практика", CalculationBase = WorkCalculationBase.PerGroup, Hours = 6m, SortOrder = 15 },
                    new NormTime { Id = 16, WorkName = "Производственная практика", CategoryName = "Практика", CalculationBase = WorkCalculationBase.PerGroup, Hours = 6m, SortOrder = 16 },
                    new NormTime { Id = 17, WorkName = "Преддипломная практика", CategoryName = "Практика", CalculationBase = WorkCalculationBase.PerStudent, Hours = 1m, SortOrder = 17 },
                    new NormTime { Id = 18, WorkName = "Ознакомительная практика", CategoryName = "Практика", CalculationBase = WorkCalculationBase.PerGroup, Hours = 6m, SortOrder = 18 },

                    new NormTime { Id = 19, WorkName = "НИР", CategoryName = "Научная работа", CalculationBase = WorkCalculationBase.PerStudent, Hours = 1m, SortOrder = 19 },
                    new NormTime { Id = 20, WorkName = "НИРМ", CategoryName = "Научная работа", CalculationBase = WorkCalculationBase.PerStudent, Hours = 1m, SortOrder = 20 }
                );
            });

            modelBuilder.Entity<AcademicPlan>(entity =>
            {
                entity.ToTable("AcademicPlans");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.EducationDirectionId)
                    .IsRequired();

                entity.Property(x => x.AcademicCourses)
                    .IsRequired();

                entity.Property(x => x.Year)
                    .IsRequired();
            });

            modelBuilder.Entity<Discipline>(entity =>
            {
                entity.ToTable("Disciplines");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DisciplineBlockId)
                    .IsRequired();

                entity.Property(x => x.DisciplineName)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(x => x.DisciplineShortName)
                    .HasMaxLength(100);

                entity.Property(x => x.DisciplineDescription)
                    .HasMaxLength(1000);

                entity.Property(x => x.DisciplineBlockBlueAsteriskName)
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<AcademicPlanRecord>(entity =>
            {
                entity.ToTable("AcademicPlanRecords");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Semester)
                    .IsRequired();

                entity.Property(x => x.Zet)
                    .IsRequired();

                entity.HasOne(x => x.AcademicPlan)
                    .WithMany(x => x.AcademicPlanRecords)
                    .HasForeignKey(x => x.AcademicPlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Discipline)
                    .WithMany(x => x.AcademicPlanRecords)
                    .HasForeignKey(x => x.DisciplineId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ParentRecord)
                    .WithMany(x => x.ChildRecords)
                    .HasForeignKey(x => x.AcademicPlanRecordParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AcademicPlanRecordElement>(entity =>
            {
                entity.ToTable("AcademicPlanRecordElements");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ActivityType)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.PlanHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.Property(x => x.FactHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.HasOne(x => x.AcademicPlanRecord)
                    .WithMany(x => x.Elements)
                    .HasForeignKey(x => x.AcademicPlanRecordId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WorkloadRow>(entity =>
            {
                entity.ToTable("WorkloadRows");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.PlanYear);

                entity.Property(x => x.AcademicPlanId);

                entity.Property(x => x.AcademicPlanRecordId);

                entity.Property(x => x.DisciplineId);

                entity.Property(x => x.DisciplineName)
                    .IsRequired()
                    .HasMaxLength(300);

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
            modelBuilder.Entity<PracticeWorkloadRow>(entity =>
            {
                entity.ToTable("PracticeWorkloadRows");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.PlanYear)
                    .IsRequired();

                entity.Property(x => x.PracticeName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.DirectionCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.DirectionName)
                    .HasMaxLength(200);

                entity.Property(x => x.Course)
                    .IsRequired();

                entity.Property(x => x.StudentsCount)
                    .IsRequired();

                entity.Property(x => x.WeeksCount)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.Property(x => x.TotalHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();
            });
        }
    }
}