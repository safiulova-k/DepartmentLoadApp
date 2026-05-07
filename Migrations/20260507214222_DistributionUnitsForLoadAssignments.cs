using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class DistributionUnitsForLoadAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LecturerLoadAssignments");

            migrationBuilder.CreateTable(
                name: "LecturerLoadAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),

                    LecturerAcademicYearPlanId = table.Column<int>(type: "integer", nullable: false),

                    SourceType = table.Column<int>(type: "integer", nullable: false),

                    SourceRowId = table.Column<int>(type: "integer", nullable: false),

                    SourceAcademicPlanRecordId = table.Column<int>(type: "integer", nullable: false),

                    LoadElementType = table.Column<int>(type: "integer", nullable: false),

                    DistributionUnitType = table.Column<int>(type: "integer", nullable: false),

                    StudentGroupId = table.Column<int>(type: "integer", nullable: true),

                    ContingentSubgroupId = table.Column<int>(type: "integer", nullable: true),

                    UnitName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),

                    StudentsCount = table.Column<int>(type: "integer", nullable: false),

                    AssignedHours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerLoadAssignments", x => x.Id);

                    table.ForeignKey(
                        name: "FK_LecturerLoadAssignments_LecturerAcademicYearPlans_LecturerAcademicYearPlanId",
                        column: x => x.LecturerAcademicYearPlanId,
                        principalTable: "LecturerAcademicYearPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LecturerLoadAssignments_LecturerAcademicYearPlanId_SourceType_SourceRowId_LoadElementType_DistributionUnitType_StudentGroupId_ContingentSubgroupId",
                table: "LecturerLoadAssignments",
                columns: new[]
                {
                    "LecturerAcademicYearPlanId",
                    "SourceType",
                    "SourceRowId",
                    "LoadElementType",
                    "DistributionUnitType",
                    "StudentGroupId",
                    "ContingentSubgroupId"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LecturerLoadAssignments_AcademicYear_SourceType_SourceRowId_LoadElementType_DistributionUnitType_StudentGroupId_ContingentSubgroupId",
                table: "LecturerLoadAssignments",
                columns: new[]
                {
                    "AcademicYear",
                    "SourceType",
                    "SourceRowId",
                    "LoadElementType",
                    "DistributionUnitType",
                    "StudentGroupId",
                    "ContingentSubgroupId"
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LecturerLoadAssignments");

            migrationBuilder.CreateTable(
                name: "LecturerLoadAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),

                    LecturerAcademicYearPlanId = table.Column<int>(type: "integer", nullable: false),

                    SourceType = table.Column<string>(type: "text", nullable: false),

                    SourceRowId = table.Column<int>(type: "integer", nullable: false),

                    SourceAcademicPlanRecordId = table.Column<int>(type: "integer", nullable: false),

                    LoadElementType = table.Column<string>(type: "text", nullable: false),

                    AssignedHours = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerLoadAssignments", x => x.Id);

                    table.ForeignKey(
                        name: "FK_LecturerLoadAssignments_LecturerAcademicYearPlans_LecturerAcademicYearPlanId",
                        column: x => x.LecturerAcademicYearPlanId,
                        principalTable: "LecturerAcademicYearPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LecturerLoadAssignments_LecturerAcademicYearPlanId",
                table: "LecturerLoadAssignments",
                column: "LecturerAcademicYearPlanId");
        }
    }
}