using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_RoutingOrders_ROLineItems_New_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeliveryPortId",
                table: "RoutingOrders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReceiptPortId",
                table: "RoutingOrders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCodeOfOrigin",
                table: "ROLineItems",
                type: "NVARCHAR(4)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryPortId",
                table: "RoutingOrders");

            migrationBuilder.DropColumn(
                name: "ReceiptPortId",
                table: "RoutingOrders");

            migrationBuilder.DropColumn(
                name: "CountryCodeOfOrigin",
                table: "ROLineItems");
        }
    }
}
