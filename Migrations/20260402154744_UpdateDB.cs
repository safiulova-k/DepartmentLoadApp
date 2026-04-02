using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentLoadApp.Migrations
{
    public partial class UpdateDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DeleteData(
                table: "NormTimes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalHours",
                table: "PracticeWorkloadRows",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalHours",
                table: "PracticeWorkloadRows",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

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
                    { 11, 3, "ГИА", 10.5m, 11, "Руководство ВКР бакалавра" },
                    { 12, 3, "ГИА", 30.5m, 12, "Руководство ВКР магистра" },
                    { 13, 3, "ГИА", 1m, 13, "Нормоконтроль ВКР" },
                    { 14, 3, "ГИА", 3.5m, 14, "Госэкзамен" },
                    { 15, 3, "ГИА", 3.5m, 21, "Работа в ГЭК" },
                    { 16, 1, "Практика", 6m, 15, "Учебная практика" },
                    { 17, 1, "Практика", 6m, 16, "Производственная практика" },
                    { 18, 3, "Практика", 1m, 17, "Преддипломная практика" },
                    { 19, 1, "Практика", 6m, 18, "Ознакомительная практика" },
                    { 20, 3, "Научная работа", 1m, 19, "НИР" },
                    { 21, 3, "Научная работа", 1m, 20, "НИРМ" }
                });
        }
    }
}
