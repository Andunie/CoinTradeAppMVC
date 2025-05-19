using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinTradeAppMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalValue",
                table: "UserPortfolio",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalValue",
                table: "UserPortfolio");
        }
    }
}
