using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequestLifeCycle.Migrations
{
    /// <inheritdoc />
    public partial class FixNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "RequestOffers",
                newName: "Id");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProposedPrice",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ServiceRequests",
                type: "longtext",
                nullable: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "OfferedPrice",
                table: "RequestOffers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RequestOffers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RequestOffers",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "RequestOffers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RequestOffers");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RequestOffers",
                newName: "id");

            migrationBuilder.AlterColumn<int>(
                name: "ProposedPrice",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "OfferedPrice",
                table: "RequestOffers",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
