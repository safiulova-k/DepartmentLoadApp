using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class ChangeAcademicYearToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PlanYear",
                table: "PracticeWorkloadRows",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "PlanYear",
                table: "GiaWorkloadRows",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Year",
                table: "AcademicPlans",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PlanYear",
                table: "PracticeWorkloadRows",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<int>(
                name: "PlanYear",
                table: "GiaWorkloadRows",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "Year",
                table: "AcademicPlans",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);
        }
    }
}
