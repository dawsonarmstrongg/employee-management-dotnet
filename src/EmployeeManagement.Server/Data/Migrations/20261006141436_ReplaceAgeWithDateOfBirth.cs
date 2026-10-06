using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAgeWithDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Employees");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Employees",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateOfBirth",
                value: new DateOnly(1991, 3, 14));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateOfBirth",
                value: new DateOnly(1980, 7, 2));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 3,
                column: "DateOfBirth",
                value: new DateOnly(1996, 11, 23));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 4,
                column: "DateOfBirth",
                value: new DateOnly(1973, 1, 30));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 5,
                column: "DateOfBirth",
                value: new DateOnly(1987, 9, 8));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Employees",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                column: "Age",
                value: 34);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                column: "Age",
                value: 45);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 3,
                column: "Age",
                value: 29);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 4,
                column: "Age",
                value: 52);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 5,
                column: "Age",
                value: 38);
        }
    }
}
