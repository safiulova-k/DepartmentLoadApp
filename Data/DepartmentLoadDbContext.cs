using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.AcademicPlan;
using DepartmentLoadApp.Models.Contingent;
using DepartmentLoadApp.Models.Gia;
using DepartmentLoadApp.Models.Practice;
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
        public DbSet<AcademicPlan> AcademicPlans => Set<AcademicPlan>();
        public DbSet<AcademicPlanRecord> AcademicPlanRecords => Set<AcademicPlanRecord>();
        public DbSet<Discipline> Disciplines => Set<Discipline>();
        public DbSet<AcademicPlanRecordElement> AcademicPlanRecordElements => Set<AcademicPlanRecordElement>();
        public DbSet<WorkloadRow> WorkloadRows => Set<WorkloadRow>();
        public DbSet<LoadCalculation> LoadCalculations => Set<LoadCalculation>();
        public DbSet<LoadDistribution> LoadDistributions => Set<LoadDistribution>();
        public DbSet<PracticeWorkloadRow> PracticeWorkloadRows => Set<PracticeWorkloadRow>();
        public DbSet<GiaWorkloadRow> GiaWorkloadRows => Set<GiaWorkloadRow>();
        public DbSet<SemesterPeriod> SemesterPeriods => Set<SemesterPeriod>();
        public DbSet<StudentFlow> StudentFlows => Set<StudentFlow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teachers");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExternalLecturerId);

                entity.Property(x => x.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Position)
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
                    .IsRequired()
                    .HasMaxLength(9);
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

                entity.Property(x => x.DisciplineBlockId)
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

                entity.Property(x => x.AcademicYear)
                    .IsRequired()
                    .HasMaxLength(9);

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

                entity.Property(x => x.HasExam).IsRequired();
                entity.Property(x => x.HasCredit).IsRequired();
                entity.Property(x => x.HasCourseWork).IsRequired();
                entity.Property(x => x.HasCourseProject).IsRequired();

                entity.Property(x => x.ConsultationHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.Property(x => x.ExamHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.Property(x => x.CreditHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.Property(x => x.CourseWorkHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.Property(x => x.CourseProjectHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();
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
                    .IsRequired()
                    .HasMaxLength(9);

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

                entity.Property(x => x.SemesterName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.EducationForm)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.StudentsCount).IsRequired();
                entity.Property(x => x.GroupCount).IsRequired();
                entity.Property(x => x.WeeksCount).IsRequired();

                entity.Property(x => x.TotalHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();
            });

            modelBuilder.Entity<GiaWorkloadRow>(entity =>
            {
                entity.ToTable("GiaWorkloadRows");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.PlanYear)
                    .IsRequired()
                    .HasMaxLength(9);

                entity.Property(x => x.GiaSection)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.WorkName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.DirectionCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.DirectionName)
                    .HasMaxLength(200);

                entity.Property(x => x.Course)
                    .IsRequired();

                entity.Property(x => x.SemesterName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.EducationForm)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.StudentsCount).IsRequired();
                entity.Property(x => x.GroupCount).IsRequired();

                entity.Property(x => x.ManualHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                entity.Property(x => x.TotalHours)
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();
            });

            modelBuilder.Entity<SemesterPeriod>(entity =>
            {
                entity.ToTable("SemesterPeriods");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.AcademicYear)
                    .IsRequired()
                    .HasMaxLength(9);

                entity.Property(x => x.Season)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.StartDate)
                    .IsRequired();

                entity.Property(x => x.EndDate)
                    .IsRequired();
            });

            modelBuilder.Entity<StudentFlow>(entity =>
            {
                entity.ToTable("StudentFlows");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.AcademicYear)
                    .IsRequired()
                    .HasMaxLength(9);

                entity.Property(x => x.FlowName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.DirectionCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Course)
                    .IsRequired();

                entity.Property(x => x.EducationLevel)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.GroupNames)
                    .HasMaxLength(500);

                entity.Property(x => x.StudentsCount)
                    .IsRequired();

                entity.Property(x => x.GroupsCount)
                    .IsRequired();
            });
        }
    }
}