using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeManagement.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false, collation: "NOCASE"),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Address1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Zip = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Age", "Email", "FirstName", "LastName", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, 34, "jane.doe@example.com", "Jane", "Doe", "(555)-123-4567" },
                    { 2, 45, "john.smith@example.com", "John", "Smith", "(555)-234-5678" },
                    { 3, 29, "maria.garcia@example.com", "Maria", "Garcia", "(555)-345-6789" },
                    { 4, 52, "david.lee@example.com", "David", "Lee", "(555)-456-7890" },
                    { 5, 38, "aisha.khan@example.com", "Aisha", "Khan", "(555)-567-8901" }
                });

            migrationBuilder.InsertData(
                table: "Addresses",
                columns: new[] { "Id", "Address1", "Address2", "City", "EmployeeId", "State", "Zip" },
                values: new object[,]
                {
                    { 1, "123 Main St", "Apt 4B", "Springfield", 1, "IL", "62701" },
                    { 2, "456 Oak Ave", null, "Austin", 2, "TX", "73301" },
                    { 3, "789 Pine Rd", "Suite 200", "Denver", 3, "CO", "80202" },
                    { 4, "321 Maple Dr", null, "Seattle", 4, "WA", "98101" },
                    { 5, "654 Elm St", null, "Raleigh", 5, "NC", "27601" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_EmployeeId",
                table: "Addresses",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
