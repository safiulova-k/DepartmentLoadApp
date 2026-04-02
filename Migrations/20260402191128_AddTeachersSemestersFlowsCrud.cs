using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddTeachersSemestersFlowsCrud : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Degree",
                table: "Teachers");

            migrationBuilder.AlterColumn<string>(
                name: "Position",
                table: "Teachers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalLecturerId",
                table: "Teachers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SemesterPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYear = table.Column<int>(type: "integer", nullable: false),
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
                    AcademicYear = table.Column<int>(type: "integer", nullable: false),
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SemesterPeriods");

            migrationBuilder.DropTable(
                name: "StudentFlows");

            migrationBuilder.DropColumn(
                name: "ExternalLecturerId",
                table: "Teachers");

            migrationBuilder.AlterColumn<string>(
                name: "Position",
                table: "Teachers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Degree",
                table: "Teachers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
