using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class UpdSemesters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanYear",
                table: "WorkloadRows");

            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "WorkloadRows",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "AcademicYear",
                table: "StudentFlows",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Year",
                table: "AcademicPlans",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "DisciplineBlockId",
                table: "AcademicPlanRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "WorkloadRows");

            migrationBuilder.DropColumn(
                name: "DisciplineBlockId",
                table: "AcademicPlanRecords");

            migrationBuilder.AddColumn<int>(
                name: "PlanYear",
                table: "WorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "AcademicYear",
                table: "StudentFlows",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<int>(
                name: "Year",
                table: "AcademicPlans",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
