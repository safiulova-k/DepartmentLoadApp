using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContingentRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DirectionCode = table.Column<string>(type: "text", nullable: false),
                    IsBachelor = table.Column<bool>(type: "boolean", nullable: false),
                    IsMaster = table.Column<bool>(type: "boolean", nullable: false),
                    Course1Count = table.Column<int>(type: "integer", nullable: false),
                    Course2Count = table.Column<int>(type: "integer", nullable: false),
                    Course3Count = table.Column<int>(type: "integer", nullable: false),
                    Course4Count = table.Column<int>(type: "integer", nullable: false),
                    Course1Groups = table.Column<int>(type: "integer", nullable: false),
                    Course2Groups = table.Column<int>(type: "integer", nullable: false),
                    Course3Groups = table.Column<int>(type: "integer", nullable: false),
                    Course4Groups = table.Column<int>(type: "integer", nullable: false),
                    Course1Subgroups = table.Column<int>(type: "integer", nullable: false),
                    Course2Subgroups = table.Column<int>(type: "integer", nullable: false),
                    Course3Subgroups = table.Column<int>(type: "integer", nullable: false),
                    Course4Subgroups = table.Column<int>(type: "integer", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContingentRows", x => x.Id);
                });

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
                    Qualification = table.Column<int>(type: "integer", nullable: false),
                    Profile = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationDirections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GiaWorkloadRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanYear = table.Column<string>(type: "text", nullable: false),
                    GiaSection = table.Column<string>(type: "text", nullable: false),
                    WorkName = table.Column<string>(type: "text", nullable: false),
                    DirectionCode = table.Column<string>(type: "text", nullable: false),
                    DirectionName = table.Column<string>(type: "text", nullable: false),
                    Course = table.Column<int>(type: "integer", nullable: false),
                    SemesterName = table.Column<string>(type: "text", nullable: false),
                    EducationForm = table.Column<string>(type: "text", nullable: false),
                    StudentsCount = table.Column<int>(type: "integer", nullable: false),
                    GroupCount = table.Column<int>(type: "integer", nullable: false),
                    ManualHours = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaWorkloadRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LecturerDepartmentPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    DepartmentPostTitle = table.Column<string>(type: "text", nullable: false),
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
                    StudyPostTitle = table.Column<string>(type: "text", nullable: false),
                    Hours = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerStudyPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoadType = table.Column<string>(type: "text", nullable: false),
                    StudentCount = table.Column<int>(type: "integer", nullable: false),
                    GroupCount = table.Column<int>(type: "integer", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadCalculations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkName = table.Column<string>(type: "text", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    CalculationBase = table.Column<int>(type: "integer", nullable: false),
                    Hours = table.Column<decimal>(type: "numeric", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormTimes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PracticeWorkloadRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanYear = table.Column<string>(type: "text", nullable: false),
                    PracticeName = table.Column<string>(type: "text", nullable: false),
                    DirectionCode = table.Column<string>(type: "text", nullable: false),
                    DirectionName = table.Column<string>(type: "text", nullable: false),
                    Course = table.Column<int>(type: "integer", nullable: false),
                    SemesterName = table.Column<string>(type: "text", nullable: false),
                    EducationForm = table.Column<string>(type: "text", nullable: false),
                    StudentsCount = table.Column<int>(type: "integer", nullable: false),
                    GroupCount = table.Column<int>(type: "integer", nullable: false),
                    WeeksCount = table.Column<int>(type: "integer", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeWorkloadRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SemesterPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Season = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemesterPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentFlows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    FlowName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DirectionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Course = table.Column<int>(type: "integer", nullable: false),
                    EducationLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GroupNames = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StudentsCount = table.Column<int>(type: "integer", nullable: false),
                    GroupsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFlows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkloadRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    AcademicPlanId = table.Column<int>(type: "integer", nullable: false),
                    AcademicPlanRecordId = table.Column<int>(type: "integer", nullable: false),
                    DisciplineId = table.Column<int>(type: "integer", nullable: false),
                    DisciplineName = table.Column<string>(type: "text", nullable: false),
                    DirectionCode = table.Column<string>(type: "text", nullable: false),
                    DirectionName = table.Column<string>(type: "text", nullable: false),
                    SemesterName = table.Column<string>(type: "text", nullable: false),
                    EducationForm = table.Column<string>(type: "text", nullable: false),
                    Course = table.Column<int>(type: "integer", nullable: false),
                    StudentsCount = table.Column<int>(type: "integer", nullable: false),
                    FlowCount = table.Column<int>(type: "integer", nullable: false),
                    GroupCount = table.Column<int>(type: "integer", nullable: false),
                    SubgroupCount = table.Column<int>(type: "integer", nullable: false),
                    LecturePlanHours = table.Column<decimal>(type: "numeric", nullable: false),
                    LectureTotalHours = table.Column<decimal>(type: "numeric", nullable: false),
                    PracticePlanHours = table.Column<decimal>(type: "numeric", nullable: false),
                    PracticeTotalHours = table.Column<decimal>(type: "numeric", nullable: false),
                    LabPlanHours = table.Column<decimal>(type: "numeric", nullable: false),
                    LabTotalHours = table.Column<decimal>(type: "numeric", nullable: false),
                    HasExam = table.Column<bool>(type: "boolean", nullable: false),
                    HasCredit = table.Column<bool>(type: "boolean", nullable: false),
                    HasCourseWork = table.Column<bool>(type: "boolean", nullable: false),
                    HasCourseProject = table.Column<bool>(type: "boolean", nullable: false),
                    ConsultationHours = table.Column<decimal>(type: "numeric", nullable: false),
                    ExamHours = table.Column<decimal>(type: "numeric", nullable: false),
                    CreditHours = table.Column<decimal>(type: "numeric", nullable: false),
                    CourseWorkHours = table.Column<decimal>(type: "numeric", nullable: false),
                    CourseProjectHours = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkloadRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcademicPlansCore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoreId = table.Column<int>(type: "integer", nullable: false),
                    EducationDirectionId = table.Column<int>(type: "integer", nullable: true),
                    EducationForm = table.Column<int>(type: "integer", nullable: false),
                    AcademicCourses = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<string>(type: "text", nullable: false)
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
                    LecturerStudyPostId = table.Column<int>(type: "integer", nullable: true),
                    LecturerDepartmentPostId = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Patronymic = table.Column<string>(type: "text", nullable: false),
                    Abbreviation = table.Column<string>(type: "text", nullable: false),
                    DateBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    HomeNumber = table.Column<string>(type: "text", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Rank2 = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Photo = table.Column<byte[]>(type: "bytea", nullable: true),
                    OnlyForPrivate = table.Column<bool>(type: "boolean", nullable: false)
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
                    PracticalHours = table.Column<int>(type: "integer", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "LoadDistributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LecturerId = table.Column<int>(type: "integer", nullable: false),
                    LoadCalculationId = table.Column<int>(type: "integer", nullable: false),
                    Hours = table.Column<decimal>(type: "numeric", nullable: false),
                    LoadCalculationId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadDistributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadDistributions_Lecturers_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Lecturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadDistributions_LoadCalculations_LoadCalculationId",
                        column: x => x.LoadCalculationId,
                        principalTable: "LoadCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoadDistributions_LoadCalculations_LoadCalculationId1",
                        column: x => x.LoadCalculationId1,
                        principalTable: "LoadCalculations",
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
                name: "IX_AcademicPlanRecordsCore_AcademicPlanId",
                table: "AcademicPlanRecordsCore",
                column: "AcademicPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicPlanRecordsCore_CoreId",
                table: "AcademicPlanRecordsCore",
                column: "CoreId",
                unique: true);

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
                name: "IX_LoadDistributions_LecturerId",
                table: "LoadDistributions",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadDistributions_LoadCalculationId",
                table: "LoadDistributions",
                column: "LoadCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadDistributions_LoadCalculationId1",
                table: "LoadDistributions",
                column: "LoadCalculationId1");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicPlanRecordsCore");

            migrationBuilder.DropTable(
                name: "ContingentRows");

            migrationBuilder.DropTable(
                name: "GiaWorkloadRows");

            migrationBuilder.DropTable(
                name: "LoadDistributions");

            migrationBuilder.DropTable(
                name: "NormTimes");

            migrationBuilder.DropTable(
                name: "PracticeWorkloadRows");

            migrationBuilder.DropTable(
                name: "SemesterPeriods");

            migrationBuilder.DropTable(
                name: "StudentFlows");

            migrationBuilder.DropTable(
                name: "StudentGroupsCore");

            migrationBuilder.DropTable(
                name: "WorkloadRows");

            migrationBuilder.DropTable(
                name: "AcademicPlansCore");

            migrationBuilder.DropTable(
                name: "LoadCalculations");

            migrationBuilder.DropTable(
                name: "Lecturers");

            migrationBuilder.DropTable(
                name: "EducationDirections");

            migrationBuilder.DropTable(
                name: "LecturerDepartmentPosts");

            migrationBuilder.DropTable(
                name: "LecturerStudyPosts");
        }
    }
}
