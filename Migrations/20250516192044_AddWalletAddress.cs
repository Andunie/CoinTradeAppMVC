using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinTradeAppMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                table: "UserMarginCash",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                table: "UserCash",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletAddress",
                table: "UserMarginCash");

            migrationBuilder.DropColumn(
                name: "WalletAddress",
                table: "UserCash");
        }
    }
}
