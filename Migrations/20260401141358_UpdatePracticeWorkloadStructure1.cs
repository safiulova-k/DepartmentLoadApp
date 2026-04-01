using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class UpdatePracticeWorkloadStructure1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WeeksCount",
                table: "PracticeWorkloadRows",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<int>(
                name: "TotalHours",
                table: "PracticeWorkloadRows",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddColumn<string>(
                name: "EducationForm",
                table: "PracticeWorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SemesterName",
                table: "PracticeWorkloadRows",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationForm",
                table: "PracticeWorkloadRows");

            migrationBuilder.DropColumn(
                name: "SemesterName",
                table: "PracticeWorkloadRows");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeeksCount",
                table: "PracticeWorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalHours",
                table: "PracticeWorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
