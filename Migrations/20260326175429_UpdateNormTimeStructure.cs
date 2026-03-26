using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class UpdateNormTimeStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "NormTimes");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "NormTimes");

            migrationBuilder.RenameColumn(
                name: "WorkTypeName",
                table: "NormTimes",
                newName: "WorkName");

            migrationBuilder.RenameColumn(
                name: "UnitName",
                table: "NormTimes",
                newName: "CategoryName");

            migrationBuilder.RenameColumn(
                name: "HoursValue",
                table: "NormTimes",
                newName: "Hours");

            migrationBuilder.RenameColumn(
                name: "CalculationType",
                table: "NormTimes",
                newName: "SortOrder");

            migrationBuilder.AddColumn<int>(
                name: "CalculationBase",
                table: "NormTimes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "NormTimes",
                columns: new[] { "Id", "CalculationBase", "CategoryName", "Hours", "SortOrder", "WorkName" },
                values: new object[,]
                {
                    { 1, 0, "Аудиторная нагрузка", 1m, 1, "Лекции" },
                    { 2, 1, "Аудиторная нагрузка", 1m, 2, "Практические занятия" },
                    { 3, 2, "Аудиторная нагрузка", 1m, 3, "Лабораторные работы" },
                    { 4, 3, "Контроль", 0.50m, 4, "Экзамены" },
                    { 5, 3, "Контроль", 0.25m, 5, "Зачеты" },
                    { 6, 4, "Контроль", 1m, 6, "Курсовая работа" },
                    { 7, 4, "Контроль", 1m, 7, "Курсовой проект" },
                    { 8, 4, "Контроль", 1m, 8, "Рефераты и РГР" },
                    { 9, 1, "Контроль", 1m, 9, "Консультации" },
                    { 10, 5, "Контроль", 1m, 10, "Консультации перед экзаменом" },
                    { 11, 4, "ВКР", 10m, 11, "Руководство ВКР бакалавра" },
                    { 12, 4, "ВКР", 15m, 12, "Руководство ВКР магистра" },
                    { 13, 4, "ВКР", 1m, 13, "Нормоконтроль ВКР" },
                    { 14, 4, "ВКР", 1m, 14, "ГосЭкзамен" },
                    { 15, 1, "Практика", 6m, 15, "Учебная практика" },
                    { 16, 1, "Практика", 6m, 16, "Производственная практика" },
                    { 17, 3, "Практика", 1m, 17, "Преддипломная практика" },
                    { 18, 1, "Практика", 6m, 18, "Ознакомительная практика" },
                    { 19, 3, "Научная работа", 1m, 19, "НИР" },
                    { 20, 3, "Научная работа", 1m, 20, "НИРМ" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DropColumn(
                name: "CalculationBase",
                table: "NormTimes");

            migrationBuilder.RenameColumn(
                name: "WorkName",
                table: "NormTimes",
                newName: "WorkTypeName");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "NormTimes",
                newName: "CalculationType");

            migrationBuilder.RenameColumn(
                name: "Hours",
                table: "NormTimes",
                newName: "HoursValue");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "NormTimes",
                newName: "UnitName");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "NormTimes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "NormTimes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
