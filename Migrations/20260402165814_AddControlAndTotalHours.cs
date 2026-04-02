using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddControlAndTotalHours : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseProjectCredits",
                table: "WorkloadRows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CourseProjectHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CourseWorkCredits",
                table: "WorkloadRows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CourseWorkHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CreditCredits",
                table: "WorkloadRows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExamConsultationHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ExamCredits",
                table: "WorkloadRows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExamHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseProjectCredits",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "CourseProjectHours",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "CourseWorkCredits",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "CourseWorkHours",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "CreditCredits",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "CreditHours",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "ExamConsultationHours",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "ExamCredits",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "ExamHours",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "WorkloadRows");
        }
    }
}
