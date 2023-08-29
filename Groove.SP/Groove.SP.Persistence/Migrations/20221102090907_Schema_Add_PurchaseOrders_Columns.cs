using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Schema_Add_PurchaseOrders_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractShipmentDate",
                table: "PurchaseOrders",
                type: "DATETIME2(7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeight",
                table: "PurchaseOrders",
                type: "DECIMAL(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                table: "PurchaseOrders",
                type: "DECIMAL(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeight",
                table: "POLineItems",
                type: "DECIMAL(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                table: "POLineItems",
                type: "DECIMAL(18,4)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractShipmentDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "GrossWeight",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "GrossWeight",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "POLineItems");
        }
    }
}
