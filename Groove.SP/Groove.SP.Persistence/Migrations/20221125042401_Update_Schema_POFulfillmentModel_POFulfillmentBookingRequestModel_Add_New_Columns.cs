using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_POFulfillmentModel_POFulfillmentBookingRequestModel_Add_New_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CFSClosingDate",
                table: "POFulfillments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CFSWarehouseCode",
                table: "POFulfillments",
                type: "NVARCHAR(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CFSWarehouseDescription",
                table: "POFulfillments",
                type: "NVARCHAR(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CYClosingDate",
                table: "POFulfillments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CYEmptyPickupTerminalCode",
                table: "POFulfillments",
                type: "NVARCHAR(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CYEmptyPickupTerminalDescription",
                table: "POFulfillments",
                type: "NVARCHAR(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CFSClosingDate",
                table: "POFulfillmentBookingRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CFSWarehouseCode",
                table: "POFulfillmentBookingRequests",
                type: "NVARCHAR(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CFSWarehouseDescription",
                table: "POFulfillmentBookingRequests",
                type: "NVARCHAR(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CYClosingDate",
                table: "POFulfillmentBookingRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CYEmptyPickupTerminalCode",
                table: "POFulfillmentBookingRequests",
                type: "NVARCHAR(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CYEmptyPickupTerminalDescription",
                table: "POFulfillmentBookingRequests",
                type: "NVARCHAR(512)",
                maxLength: 512,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CFSClosingDate",
                table: "POFulfillments");

            migrationBuilder.DropColumn(
                name: "CFSWarehouseCode",
                table: "POFulfillments");

            migrationBuilder.DropColumn(
                name: "CFSWarehouseDescription",
                table: "POFulfillments");

            migrationBuilder.DropColumn(
                name: "CYClosingDate",
                table: "POFulfillments");

            migrationBuilder.DropColumn(
                name: "CYEmptyPickupTerminalCode",
                table: "POFulfillments");

            migrationBuilder.DropColumn(
                name: "CYEmptyPickupTerminalDescription",
                table: "POFulfillments");

            migrationBuilder.DropColumn(
                name: "CFSClosingDate",
                table: "POFulfillmentBookingRequests");

            migrationBuilder.DropColumn(
                name: "CFSWarehouseCode",
                table: "POFulfillmentBookingRequests");

            migrationBuilder.DropColumn(
                name: "CFSWarehouseDescription",
                table: "POFulfillmentBookingRequests");

            migrationBuilder.DropColumn(
                name: "CYClosingDate",
                table: "POFulfillmentBookingRequests");

            migrationBuilder.DropColumn(
                name: "CYEmptyPickupTerminalCode",
                table: "POFulfillmentBookingRequests");

            migrationBuilder.DropColumn(
                name: "CYEmptyPickupTerminalDescription",
                table: "POFulfillmentBookingRequests");
        }
    }
}
