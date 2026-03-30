using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class UpdateWorkloadRowStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicPlanId",
                table: "WorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademicPlanRecordId",
                table: "WorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisciplineId",
                table: "WorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisciplineName",
                table: "WorkloadRows",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PlanYear",
                table: "WorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AcademicPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EducationDirectionId = table.Column<int>(type: "integer", nullable: false),
                    AcademicCourses = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisciplineBlockId = table.Column<int>(type: "integer", nullable: false),
                    DisciplineName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DisciplineShortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisciplineDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisciplineBlockBlueAsteriskName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcademicPlanRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicPlanId = table.Column<int>(type: "integer", nullable: false),
                    DisciplineId = table.Column<int>(type: "integer", nullable: false),
                    AcademicPlanRecordParentId = table.Column<int>(type: "integer", nullable: true),
                    InDepartment = table.Column<bool>(type: "boolean", nullable: false),
                    Semester = table.Column<int>(type: "integer", nullable: false),
                    Zet = table.Column<int>(type: "integer", nullable: false),
                    IsParent = table.Column<bool>(type: "boolean", nullable: false),
                    IsChild = table.Column<bool>(type: "boolean", nullable: false),
                    IsFacultative = table.Column<bool>(type: "boolean", nullable: false),
                    IsUseInWorkload = table.Column<bool>(type: "boolean", nullable: false),
                    IsActiveSemester = table.Column<bool>(type: "boolean", nullable: false)
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
                    PlanHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    FactHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicPlanRecordElements");

            migrationBuilder.DropTable(
                name: "AcademicPlanRecords");

            migrationBuilder.DropTable(
                name: "AcademicPlans");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropColumn(
                name: "AcademicPlanId",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "AcademicPlanRecordId",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "DisciplineId",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "DisciplineName",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "PlanYear",
                table: "WorkloadRows");
        }
    }
}
