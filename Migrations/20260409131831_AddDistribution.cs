using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddDistribution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LecturerAcademicYearPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    LecturerId = table.Column<int>(type: "integer", nullable: false),
                    LecturerStudyPostId = table.Column<int>(type: "integer", nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerAcademicYearPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LecturerAcademicYearPlans_Lecturers_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Lecturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LecturerAcademicYearPlans_LecturerStudyPosts_LecturerStudyP~",
                        column: x => x.LecturerStudyPostId,
                        principalTable: "LecturerStudyPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    LoadElementType = table.Column<string>(type: "text", nullable: false),
                    AssignedHours = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerLoadAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LecturerLoadAssignments_LecturerAcademicYearPlans_LecturerA~",
                        column: x => x.LecturerAcademicYearPlanId,
                        principalTable: "LecturerAcademicYearPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LecturerAcademicYearPlans_AcademicYear_LecturerId",
                table: "LecturerAcademicYearPlans",
                columns: new[] { "AcademicYear", "LecturerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LecturerAcademicYearPlans_LecturerId",
                table: "LecturerAcademicYearPlans",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_LecturerAcademicYearPlans_LecturerStudyPostId",
                table: "LecturerAcademicYearPlans",
                column: "LecturerStudyPostId");

            migrationBuilder.CreateIndex(
                name: "IX_LecturerLoadAssignments_AcademicYear_SourceType_SourceRowId~",
                table: "LecturerLoadAssignments",
                columns: new[] { "AcademicYear", "SourceType", "SourceRowId", "LoadElementType" });

            migrationBuilder.CreateIndex(
                name: "IX_LecturerLoadAssignments_LecturerAcademicYearPlanId_SourceTy~",
                table: "LecturerLoadAssignments",
                columns: new[] { "LecturerAcademicYearPlanId", "SourceType", "SourceRowId", "LoadElementType" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LecturerLoadAssignments");

            migrationBuilder.DropTable(
                name: "LecturerAcademicYearPlans");
        }
    }
}
