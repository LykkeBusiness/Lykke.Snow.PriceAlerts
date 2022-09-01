using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lykke.Snow.PriceAlerts.SqlRepositories.Migrations
{
    public partial class IncreasePrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "priceAlerts",
                table: "PriceAlerts",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "priceAlerts",
                table: "PriceAlerts",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");
        }
    }
}
