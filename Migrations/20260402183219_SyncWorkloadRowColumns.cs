using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class SyncWorkloadRowColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseProjectCredits",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "CourseWorkCredits",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "CreditCredits",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "ExamConsultationHours",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "ExamCredits",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "RoundingType",
                table: "NormTimes");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExamHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

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

            migrationBuilder.AddColumn<decimal>(
                name: "ConsultationHours",
                table: "WorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "HasCourseProject",
                table: "WorkloadRows",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasCourseWork",
                table: "WorkloadRows",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasCredit",
                table: "WorkloadRows",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasExam",
                table: "WorkloadRows",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultationHours",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "HasCourseProject",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "HasCourseWork",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "HasCredit",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "HasExam",
                table: "WorkloadRows");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExamHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

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

            migrationBuilder.AddColumn<int>(
                name: "CourseProjectCredits",
                table: "WorkloadRows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseWorkCredits",
                table: "WorkloadRows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreditCredits",
                table: "WorkloadRows",
                type: "integer",
                nullable: true);

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
                name: "TotalHours",
                table: "WorkloadRows",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RoundingType",
                table: "NormTimes",
                type: "text",
                nullable: true);
        }
    }
}
