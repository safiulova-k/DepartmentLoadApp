using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class CoreIntegrationInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadDistributions_Teachers_TeacherId",
                table: "LoadDistributions");

            migrationBuilder.DropTable(
                name: "AcademicPlanRecordElements");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "AcademicPlanRecords");

            migrationBuilder.DropTable(
                name: "AcademicPlans");

            migrationBuilder.DropIndex(
                name: "IX_LoadDistributions_TeacherId",
                table: "LoadDistributions");

            migrationBuilder.AlterColumn<string>(
                name: "SemesterName",
                table: "WorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "PracticeTotalHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PracticePlanHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LectureTotalHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LecturePlanHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LabTotalHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LabPlanHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExamHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "EducationForm",
                table: "WorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "DisciplineName",
                table: "WorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "DirectionName",
                table: "WorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DirectionCode",
                table: "WorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "CreditHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CourseWorkHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CourseProjectHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ConsultationHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalHours",
                table: "PracticeWorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "SemesterName",
                table: "PracticeWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PracticeName",
                table: "PracticeWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "PlanYear",
                table: "PracticeWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "EducationForm",
                table: "PracticeWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "DirectionName",
                table: "PracticeWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DirectionCode",
                table: "PracticeWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "WorkName",
                table: "NormTimes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "Hours",
                table: "NormTimes",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "NormTimes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "Hours",
                table: "LoadDistributions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddColumn<int>(
                name: "LecturerId",
                table: "LoadDistributions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalHours",
                table: "LoadCalculations",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "LoadType",
                table: "LoadCalculations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "WorkName",
                table: "GiaWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalHours",
                table: "GiaWorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "SemesterName",
                table: "GiaWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PlanYear",
                table: "GiaWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<decimal>(
                name: "ManualHours",
                table: "GiaWorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "GiaSection",
                table: "GiaWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EducationForm",
                table: "GiaWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "DirectionName",
                table: "GiaWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DirectionCode",
                table: "GiaWorkloadRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "DirectionCode",
                table: "ContingentRows",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "EducationDirections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    Cipher = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Qualification = table.Column<string>(type: "text", nullable: false),
                    Profile = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationDirections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LecturerDepartmentPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerDepartmentPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LecturerStudyPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Hours = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerStudyPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcademicPlansCore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    EducationDirectionId = table.Column<int>(type: "integer", nullable: false),
                    EducationForm = table.Column<string>(type: "text", nullable: false),
                    AcademicCourses = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlansCore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicPlansCore_EducationDirections_EducationDirectionId",
                        column: x => x.EducationDirectionId,
                        principalTable: "EducationDirections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lecturers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: false),
                    LecturerStudyPostId = table.Column<int>(type: "integer", nullable: false),
                    LecturerDepartmentPostId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lecturers_LecturerDepartmentPosts_LecturerDepartmentPostId",
                        column: x => x.LecturerDepartmentPostId,
                        principalTable: "LecturerDepartmentPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lecturers_LecturerStudyPosts_LecturerStudyPostId",
                        column: x => x.LecturerStudyPostId,
                        principalTable: "LecturerStudyPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AcademicPlanRecordsCore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    AcademicPlanId = table.Column<int>(type: "integer", nullable: false),
                    Index = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Semester = table.Column<int>(type: "integer", nullable: false),
                    Zet = table.Column<int>(type: "integer", nullable: false),
                    AcademicHours = table.Column<int>(type: "integer", nullable: false),
                    Exam = table.Column<int>(type: "integer", nullable: true),
                    Pass = table.Column<int>(type: "integer", nullable: true),
                    GradedPass = table.Column<int>(type: "integer", nullable: true),
                    CourseWork = table.Column<int>(type: "integer", nullable: true),
                    CourseProject = table.Column<int>(type: "integer", nullable: true),
                    Rgr = table.Column<int>(type: "integer", nullable: true),
                    Lectures = table.Column<int>(type: "integer", nullable: true),
                    LaboratoryHours = table.Column<int>(type: "integer", nullable: true),
                    PracticalHours = table.Column<int>(type: "integer", nullable: true),
                    DisciplineId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlanRecordsCore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicPlanRecordsCore_AcademicPlansCore_AcademicPlanId",
                        column: x => x.AcademicPlanId,
                        principalTable: "AcademicPlansCore",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcademicPlanRecordsCore_Disciplines_DisciplineId",
                        column: x => x.DisciplineId,
                        principalTable: "Disciplines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentGroupsCore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    EducationDirectionId = table.Column<int>(type: "integer", nullable: false),
                    CuratorId = table.Column<int>(type: "integer", nullable: true),
                    GroupName = table.Column<string>(type: "text", nullable: false),
                    Course = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGroupsCore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentGroupsCore_EducationDirections_EducationDirectionId",
                        column: x => x.EducationDirectionId,
                        principalTable: "EducationDirections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentGroupsCore_Lecturers_CuratorId",
                        column: x => x.CuratorId,
                        principalTable: "Lecturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadDistributions_LecturerId",
                table: "LoadDistributions",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlanRecordsCore_AcademicPlanId",
                table: "AcademicPlanRecordsCore",
                column: "AcademicPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlanRecordsCore_CoreId",
                table: "AcademicPlanRecordsCore",
                column: "CoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlanRecordsCore_DisciplineId",
                table: "AcademicPlanRecordsCore",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlansCore_CoreId",
                table: "AcademicPlansCore",
                column: "CoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlansCore_EducationDirectionId",
                table: "AcademicPlansCore",
                column: "EducationDirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationDirections_CoreId",
                table: "EducationDirections",
                column: "CoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LecturerDepartmentPosts_CoreId",
                table: "LecturerDepartmentPosts",
                column: "CoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_CoreId",
                table: "Lecturers",
                column: "CoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_LecturerDepartmentPostId",
                table: "Lecturers",
                column: "LecturerDepartmentPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_LecturerStudyPostId",
                table: "Lecturers",
                column: "LecturerStudyPostId");

            migrationBuilder.CreateIndex(
                name: "IX_LecturerStudyPosts_CoreId",
                table: "LecturerStudyPosts",
                column: "CoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroupsCore_CoreId",
                table: "StudentGroupsCore",
                column: "CoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroupsCore_CuratorId",
                table: "StudentGroupsCore",
                column: "CuratorId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroupsCore_EducationDirectionId",
                table: "StudentGroupsCore",
                column: "EducationDirectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadDistributions_Lecturers_LecturerId",
                table: "LoadDistributions",
                column: "LecturerId",
                principalTable: "Lecturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadDistributions_Lecturers_LecturerId",
                table: "LoadDistributions");

            migrationBuilder.DropTable(
                name: "AcademicPlanRecordsCore");

            migrationBuilder.DropTable(
                name: "StudentGroupsCore");

            migrationBuilder.DropTable(
                name: "AcademicPlansCore");

            migrationBuilder.DropTable(
                name: "Lecturers");

            migrationBuilder.DropTable(
                name: "EducationDirections");

            migrationBuilder.DropTable(
                name: "LecturerDepartmentPosts");

            migrationBuilder.DropTable(
                name: "LecturerStudyPosts");

            migrationBuilder.DropIndex(
                name: "IX_LoadDistributions_LecturerId",
                table: "LoadDistributions");

            migrationBuilder.DropColumn(
                name: "LecturerId",
                table: "LoadDistributions");

            migrationBuilder.AlterColumn<string>(
                name: "SemesterName",
                table: "WorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "PracticeTotalHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "PracticePlanHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "LectureTotalHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "LecturePlanHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "LabTotalHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "LabPlanHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExamHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "EducationForm",
                table: "WorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DisciplineName",
                table: "WorkloadRows",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DirectionName",
                table: "WorkloadRows",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DirectionCode",
                table: "WorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "CreditHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "CourseWorkHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "CourseProjectHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "ConsultationHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalHours",
                table: "PracticeWorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "SemesterName",
                table: "PracticeWorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PracticeName",
                table: "PracticeWorkloadRows",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PlanYear",
                table: "PracticeWorkloadRows",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EducationForm",
                table: "PracticeWorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DirectionName",
                table: "PracticeWorkloadRows",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DirectionCode",
                table: "PracticeWorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "WorkName",
                table: "NormTimes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Hours",
                table: "NormTimes",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "NormTimes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Hours",
                table: "LoadDistributions",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalHours",
                table: "LoadCalculations",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "LoadType",
                table: "LoadCalculations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "WorkName",
                table: "GiaWorkloadRows",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalHours",
                table: "GiaWorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "SemesterName",
                table: "GiaWorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PlanYear",
                table: "GiaWorkloadRows",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "ManualHours",
                table: "GiaWorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "GiaSection",
                table: "GiaWorkloadRows",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EducationForm",
                table: "GiaWorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DirectionName",
                table: "GiaWorkloadRows",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DirectionCode",
                table: "GiaWorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DirectionCode",
                table: "ContingentRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "AcademicPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicCourses = table.Column<int>(type: "integer", nullable: false),
                    EducationDirectionId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalLecturerId = table.Column<int>(type: "integer", nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcademicPlanRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicPlanId = table.Column<int>(type: "integer", nullable: false),
                    AcademicPlanRecordParentId = table.Column<int>(type: "integer", nullable: true),
                    DisciplineId = table.Column<int>(type: "integer", nullable: false),
                    DisciplineBlockId = table.Column<int>(type: "integer", nullable: false),
                    InDepartment = table.Column<bool>(type: "boolean", nullable: false),
                    IsActiveSemester = table.Column<bool>(type: "boolean", nullable: false),
                    IsChild = table.Column<bool>(type: "boolean", nullable: false),
                    IsFacultative = table.Column<bool>(type: "boolean", nullable: false),
                    IsParent = table.Column<bool>(type: "boolean", nullable: false),
                    IsUseInWorkload = table.Column<bool>(type: "boolean", nullable: false),
                    Semester = table.Column<int>(type: "integer", nullable: false),
                    Zet = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlanRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicPlanRecords_AcademicPlanRecords_AcademicPlanRecordP~",
                        column: x => x.AcademicPlanRecordParentId,
                        principalTable: "AcademicPlanRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicPlanRecords_AcademicPlans_AcademicPlanId",
                        column: x => x.AcademicPlanId,
                        principalTable: "AcademicPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcademicPlanRecords_Disciplines_DisciplineId",
                        column: x => x.DisciplineId,
                        principalTable: "Disciplines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AcademicPlanRecordElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicPlanRecordId = table.Column<int>(type: "integer", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FactHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PlanHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlanRecordElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicPlanRecordElements_AcademicPlanRecords_AcademicPlan~",
                        column: x => x.AcademicPlanRecordId,
                        principalTable: "AcademicPlanRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadDistributions_TeacherId",
                table: "LoadDistributions",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlanRecordElements_AcademicPlanRecordId",
                table: "AcademicPlanRecordElements",
                column: "AcademicPlanRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlanRecords_AcademicPlanId",
                table: "AcademicPlanRecords",
                column: "AcademicPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlanRecords_AcademicPlanRecordParentId",
                table: "AcademicPlanRecords",
                column: "AcademicPlanRecordParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlanRecords_DisciplineId",
                table: "AcademicPlanRecords",
                column: "DisciplineId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadDistributions_Teachers_TeacherId",
                table: "LoadDistributions",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
