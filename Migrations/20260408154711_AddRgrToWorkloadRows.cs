using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddRgrToWorkloadRows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasRgr",
                table: "WorkloadRows",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFacultyOptional",
                table: "WorkloadRows",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecordIndex",
                table: "WorkloadRows",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RgrHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasRgr",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "IsFacultyOptional",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "RecordIndex",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "RgrHours",
                table: "WorkloadRows");
        }
    }
}
