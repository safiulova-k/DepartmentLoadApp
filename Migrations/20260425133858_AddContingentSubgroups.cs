using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddContingentSubgroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContingentSubgroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentGroupId = table.Column<int>(type: "integer", nullable: false),
                    SubgroupNumber = table.Column<int>(type: "integer", nullable: false),
                    StudentsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContingentSubgroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContingentSubgroups_StudentGroupsCore_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroupsCore",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContingentSubgroups_StudentGroupId_SubgroupNumber",
                table: "ContingentSubgroups",
                columns: new[] { "StudentGroupId", "SubgroupNumber" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContingentSubgroups");
        }
    }
}