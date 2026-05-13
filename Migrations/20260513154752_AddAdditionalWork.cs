using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddAdditionalWork : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalWorkNorms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkType = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Hours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalWorkNorms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalWorkloadRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYear = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WorkType = table.Column<int>(type: "integer", nullable: false),
                    AdditionalWorkNormId = table.Column<int>(type: "integer", nullable: true),
                    WorkName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    HoursPerUnit = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalHours = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalWorkloadRows", x => x.Id);

                    table.ForeignKey(
                        name: "FK_AdditionalWorkloadRows_AdditionalWorkNorms_AdditionalWorkNormId",
                        column: x => x.AdditionalWorkNormId,
                        principalTable: "AdditionalWorkNorms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalWorkNorms_Code",
                table: "AdditionalWorkNorms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalWorkloadRows_AcademicYear",
                table: "AdditionalWorkloadRows",
                column: "AcademicYear");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalWorkloadRows_AdditionalWorkNormId",
                table: "AdditionalWorkloadRows",
                column: "AdditionalWorkNormId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalWorkloadRows");

            migrationBuilder.DropTable(
                name: "AdditionalWorkNorms");
        }
    }
}