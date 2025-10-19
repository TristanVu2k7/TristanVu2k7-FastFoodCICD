using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment_NET104.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "OrderHistories");

            migrationBuilder.RenameColumn(
                name: "DateAdded",
                table: "OrderHistories",
                newName: "OrderDate");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "OrderHistories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "OrderHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "OrderHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "OrderHistories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "OrderHistories");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "OrderHistories");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "OrderHistories");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "OrderHistories");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "OrderHistories",
                newName: "DateAdded");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OrderHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
