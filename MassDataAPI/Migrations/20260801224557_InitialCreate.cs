using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassDataAPI.Migrations
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Gender = table.Column<string>(type: "TEXT", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Salary = table.Column<decimal>(type: "REAL", nullable: false),
                    JoinDate = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Age",
                table: "Employees",
                column: "Age");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Country",
                table: "Employees",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Country_Department",
                table: "Employees",
                columns: new[] { "Country", "Department" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Department",
                table: "Employees",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_JoinDate",
                table: "Employees",
                column: "JoinDate");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Salary",
                table: "Employees",
                column: "Salary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
