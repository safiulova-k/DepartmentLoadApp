using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class InitMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContingentRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DirectionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                name: "LoadCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoadType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StudentCount = table.Column<int>(type: "integer", nullable: false),
                    GroupCount = table.Column<int>(type: "integer", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
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
                    WorkTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CalculationType = table.Column<int>(type: "integer", nullable: false),
                    UnitName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HoursValue = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormTimes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Degree = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkloadRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DirectionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DirectionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SemesterName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EducationForm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Course = table.Column<int>(type: "integer", nullable: false),
                    StudentsCount = table.Column<int>(type: "integer", nullable: false),
                    FlowCount = table.Column<int>(type: "integer", nullable: false),
                    GroupCount = table.Column<int>(type: "integer", nullable: false),
                    SubgroupCount = table.Column<int>(type: "integer", nullable: false),
                    LecturePlanHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PracticePlanHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LabPlanHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LectureTotalHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PracticeTotalHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LabTotalHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkloadRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadDistributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    LoadCalculationId = table.Column<int>(type: "integer", nullable: false),
                    Hours = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadDistributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadDistributions_LoadCalculations_LoadCalculationId",
                        column: x => x.LoadCalculationId,
                        principalTable: "LoadCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoadDistributions_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadDistributions_LoadCalculationId",
                table: "LoadDistributions",
                column: "LoadCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadDistributions_TeacherId",
                table: "LoadDistributions",
                column: "TeacherId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContingentRows");

            migrationBuilder.DropTable(
                name: "LoadDistributions");

            migrationBuilder.DropTable(
                name: "NormTimes");

            migrationBuilder.DropTable(
                name: "WorkloadRows");

            migrationBuilder.DropTable(
                name: "LoadCalculations");

            migrationBuilder.DropTable(
                name: "Teachers");
        }
    }
}
