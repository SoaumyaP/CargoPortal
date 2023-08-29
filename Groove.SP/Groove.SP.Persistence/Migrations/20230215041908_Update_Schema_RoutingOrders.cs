using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_RoutingOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredCarrier",
                table: "RoutingOrders");

            migrationBuilder.DropColumn(
                name: "ShipFromName",
                table: "RoutingOrders");

            migrationBuilder.DropColumn(
                name: "ShipToName",
                table: "RoutingOrders");

            migrationBuilder.RenameColumn(
                name: "ShipTo",
                table: "RoutingOrders",
                newName: "ShipToId");

            migrationBuilder.RenameColumn(
                name: "ShipFrom",
                table: "RoutingOrders",
                newName: "ShipFromId");

            migrationBuilder.RenameColumn(
                name: "PONumber",
                table: "ROLineItems",
                newName: "PONo");

            migrationBuilder.AddColumn<long>(
                name: "CarrierId",
                table: "RoutingOrders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "RoutingOrderContacts",
                type: "NVARCHAR(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(40)",
                oldMaxLength: 40,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarrierId",
                table: "RoutingOrders");

            migrationBuilder.RenameColumn(
                name: "ShipToId",
                table: "RoutingOrders",
                newName: "ShipTo");

            migrationBuilder.RenameColumn(
                name: "ShipFromId",
                table: "RoutingOrders",
                newName: "ShipFrom");

            migrationBuilder.RenameColumn(
                name: "PONo",
                table: "ROLineItems",
                newName: "PONumber");

            migrationBuilder.AddColumn<long>(
                name: "PreferredCarrier",
                table: "RoutingOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ShipFromName",
                table: "RoutingOrders",
                type: "NVARCHAR(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToName",
                table: "RoutingOrders",
                type: "NVARCHAR(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "RoutingOrderContacts",
                type: "NVARCHAR(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
