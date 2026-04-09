using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddAcademicPlanIdsToPracticeAndGia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicPlanId",
                table: "PracticeWorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademicPlanRecordId",
                table: "PracticeWorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademicPlanId",
                table: "GiaWorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademicPlanRecordId",
                table: "GiaWorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicPlanId",
                table: "PracticeWorkloadRows");

            migrationBuilder.DropColumn(
                name: "AcademicPlanRecordId",
                table: "PracticeWorkloadRows");

            migrationBuilder.DropColumn(
                name: "AcademicPlanId",
                table: "GiaWorkloadRows");

            migrationBuilder.DropColumn(
                name: "AcademicPlanRecordId",
                table: "GiaWorkloadRows");
        }
    }
}
