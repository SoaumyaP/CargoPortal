using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class New_Schema_POFulfillmentShortshipOrderModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "POFulfillmentShortshipOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POFulfillmentId = table.Column<long>(type: "bigint", nullable: false),
                    POFulfillmentNumber = table.Column<string>(type: "NVARCHAR(20)", maxLength: 20, nullable: false),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerPONumber = table.Column<string>(type: "NVARCHAR(512)", maxLength: 512, nullable: true),
                    ProductCode = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    OrderedUnitQty = table.Column<int>(type: "int", nullable: false),
                    FulfillmentUnitQty = table.Column<int>(type: "int", nullable: false),
                    BalanceUnitQty = table.Column<int>(type: "int", nullable: false),
                    BookedPackage = table.Column<int>(type: "int", nullable: true),
                    Volume = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    PolicyLog = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POFulfillmentShortshipOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_POFulfillmentShortshipOrders_POFulfillments_POFulfillmentId",
                        column: x => x.POFulfillmentId,
                        principalTable: "POFulfillments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_POFulfillmentShortshipOrders_POFulfillmentId",
                table: "POFulfillmentShortshipOrders",
                column: "POFulfillmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POFulfillmentShortshipOrders");
        }
    }
}
