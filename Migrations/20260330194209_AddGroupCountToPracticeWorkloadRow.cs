using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddGroupCountToPracticeWorkloadRow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupCount",
                table: "PracticeWorkloadRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupCount",
                table: "PracticeWorkloadRows");
        }
    }
}
