using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Schema_Add_Columns_Table_POLineItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FactoryName",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GridValue",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderText",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "POLineItems",
                type: "DECIMAL(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InboundDelivery",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                table: "POLineItems",
                type: "DECIMAL(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatGrpDe",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialType",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                table: "POLineItems",
                type: "DECIMAL(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "POItemReference",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleLineNo",
                table: "POLineItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentNo",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StockCategory",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLocation",
                table: "POLineItems",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "POLineItems",
                type: "DECIMAL(18,4)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FactoryName",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "GridValue",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "HeaderText",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "InboundDelivery",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "MatGrpDe",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "MaterialType",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "POItemReference",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "Plant",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "ScheduleLineNo",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "ShipmentNo",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "StockCategory",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "StorageLocation",
                table: "POLineItems");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "POLineItems");
        }
    }
}
