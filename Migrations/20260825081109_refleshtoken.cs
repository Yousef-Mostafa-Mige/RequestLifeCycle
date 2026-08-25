using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequestLifeCycle.Migrations
{
    /// <inheritdoc />
    public partial class refleshtoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserType",
                table: "users",
                newName: "Role");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "users",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "users",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "users",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "users",
                newName: "UserType");
        }
    }
}
