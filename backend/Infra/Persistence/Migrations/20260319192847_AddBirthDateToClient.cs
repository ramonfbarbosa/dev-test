using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthDateToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Client",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE `Client` SET `BirthDate` = '1900-01-01 00:00:00' WHERE `BirthDate` IS NULL;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Client",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                column: "PasswordHash",
                value: "$2a$11$afbFskWwfrnyrP581D4yQOQIZKg5rkhEuuJETXwHlCgnchNi106.6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Client");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                column: "PasswordHash",
                value: "$2a$11$5lKCgo.cvqYJsgdqCiGe5OUzCQv54Ngq4Y6dbG6rg1cqUF2Zy79Sa");
        }
    }
}
