using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class AddGiaCalculation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiaWorkloadRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanYear = table.Column<int>(type: "integer", nullable: false),
                    GiaSection = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WorkName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DirectionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DirectionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Course = table.Column<int>(type: "integer", nullable: false),
                    SemesterName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EducationForm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StudentsCount = table.Column<int>(type: "integer", nullable: false),
                    GroupCount = table.Column<int>(type: "integer", nullable: false),
                    ManualHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaWorkloadRows", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CalculationBase", "CategoryName", "Hours" },
                values: new object[] { 3, "ГИА", 10.5m });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CalculationBase", "CategoryName", "Hours" },
                values: new object[] { 3, "ГИА", 30.5m });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CalculationBase", "CategoryName" },
                values: new object[] { 3, "ГИА" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CalculationBase", "CategoryName", "Hours", "WorkName" },
                values: new object[] { 3, "ГИА", 3.5m, "Госэкзамен" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CalculationBase", "CategoryName", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 3, "ГИА", 3.5m, 21, "Работа в ГЭК" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "SortOrder", "WorkName" },
                values: new object[] { 15, "Учебная практика" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CalculationBase", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 1, 6m, 16, "Производственная практика" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CalculationBase", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 3, 1m, 17, "Преддипломная практика" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CalculationBase", "CategoryName", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 1, "Практика", 6m, 18, "Ознакомительная практика" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "SortOrder", "WorkName" },
                values: new object[] { 19, "НИР" });

            migrationBuilder.InsertData(
                table: "NormTimes",
                columns: new[] { "Id", "CalculationBase", "CategoryName", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 21, 3, "Научная работа", 1m, 20, "НИРМ" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiaWorkloadRows");

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CalculationBase", "CategoryName", "Hours" },
                values: new object[] { 4, "ВКР", 10m });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CalculationBase", "CategoryName", "Hours" },
                values: new object[] { 4, "ВКР", 15m });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CalculationBase", "CategoryName" },
                values: new object[] { 4, "ВКР" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CalculationBase", "CategoryName", "Hours", "WorkName" },
                values: new object[] { 4, "ВКР", 1m, "ГосЭкзамен" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CalculationBase", "CategoryName", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 1, "Практика", 6m, 15, "Учебная практика" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "SortOrder", "WorkName" },
                values: new object[] { 16, "Производственная практика" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CalculationBase", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 3, 1m, 17, "Преддипломная практика" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CalculationBase", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 1, 6m, 18, "Ознакомительная практика" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CalculationBase", "CategoryName", "Hours", "SortOrder", "WorkName" },
                values: new object[] { 3, "Научная работа", 1m, 19, "НИР" });

            migrationBuilder.UpdateData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "SortOrder", "WorkName" },
                values: new object[] { 20, "НИРМ" });
        }
    }
}
