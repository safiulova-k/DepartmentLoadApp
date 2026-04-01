using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddPracticeWorkload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PracticeWorkloadRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanYear = table.Column<int>(type: "integer", nullable: false),
                    PracticeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DirectionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DirectionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Course = table.Column<int>(type: "integer", nullable: false),
                    StudentsCount = table.Column<int>(type: "integer", nullable: false),
                    WeeksCount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeWorkloadRows", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PracticeWorkloadRows");
        }
    }
}
