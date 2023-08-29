using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Add_New_Schema_RoutingOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoutingOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoutingOrderNumber = table.Column<string>(type: "VARCHAR(20)", maxLength: 20, nullable: false),
                    RoutingOrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CargoReadyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    NumberOfLineItems = table.Column<long>(type: "bigint", nullable: false),
                    Incoterm = table.Column<int>(type: "int", nullable: false),
                    ModeOfTransport = table.Column<int>(type: "int", nullable: false),
                    LogisticsService = table.Column<int>(type: "int", nullable: true),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    ShipFrom = table.Column<long>(type: "bigint", nullable: false),
                    ShipTo = table.Column<long>(type: "bigint", nullable: false),
                    ShipFromName = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    ShipToName = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    EarliestShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedShipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LatestShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastDateForShipment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreferredCarrier = table.Column<long>(type: "bigint", nullable: false),
                    VesselName = table.Column<string>(type: "NVARCHAR(512)", maxLength: 512, nullable: true),
                    VoyageNo = table.Column<string>(type: "NVARCHAR(512)", maxLength: 512, nullable: true),
                    IsContainDangerousGoods = table.Column<bool>(type: "bit", nullable: false),
                    IsBatteryOrChemical = table.Column<bool>(type: "bit", nullable: false),
                    IsCIQOrFumigation = table.Column<bool>(type: "bit", nullable: false),
                    IsExportLicence = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ROLineItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoutingOrderId = table.Column<long>(type: "bigint", nullable: false),
                    PONumber = table.Column<string>(type: "NVARCHAR(512)", maxLength: 512, nullable: true),
                    ItemNo = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                    DescriptionOfGoods = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    ChineseDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderedUnitQty = table.Column<int>(type: "int", nullable: false),
                    UnitUOM = table.Column<int>(type: "int", nullable: false),
                    BookedPackage = table.Column<int>(type: "int", nullable: true),
                    PackageUOM = table.Column<int>(type: "int", nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Volume = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    HsCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Commodity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingMarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ROLineItems_RoutingOrders_RoutingOrderId",
                        column: x => x.RoutingOrderId,
                        principalTable: "RoutingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutingOrderContacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoutingOrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    OrganizationCode = table.Column<string>(type: "VARCHAR(35)", maxLength: 35, nullable: false),
                    OrganizationRole = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    CompanyName = table.Column<string>(type: "NVARCHAR(100)", maxLength: 100, nullable: false),
                    AddressLine1 = table.Column<string>(type: "NVARCHAR(50)", maxLength: 50, nullable: true),
                    AddressLine2 = table.Column<string>(type: "NVARCHAR(50)", maxLength: 50, nullable: true),
                    AddressLine3 = table.Column<string>(type: "NVARCHAR(50)", maxLength: 50, nullable: true),
                    AddressLine4 = table.Column<string>(type: "NVARCHAR(50)", maxLength: 50, nullable: true),
                    ContactName = table.Column<string>(type: "NVARCHAR(30)", maxLength: 30, nullable: true),
                    ContactNumber = table.Column<string>(type: "NVARCHAR(30)", maxLength: 30, nullable: true),
                    ContactEmail = table.Column<string>(type: "NVARCHAR(40)", maxLength: 40, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingOrderContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingOrderContacts_RoutingOrders_RoutingOrderId",
                        column: x => x.RoutingOrderId,
                        principalTable: "RoutingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutingOrderContainers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoutingOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ContainerType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Volume = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingOrderContainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingOrderContainers_RoutingOrders_RoutingOrderId",
                        column: x => x.RoutingOrderId,
                        principalTable: "RoutingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutingOrderInvoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoutingOrderId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceType = table.Column<string>(type: "NVARCHAR(50)", maxLength: 50, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "VARCHAR(35)", maxLength: 35, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingOrderInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingOrderInvoices_RoutingOrders_RoutingOrderId",
                        column: x => x.RoutingOrderId,
                        principalTable: "RoutingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ROLineItems_RoutingOrderId",
                table: "ROLineItems",
                column: "RoutingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingOrderContacts_OrganizationId",
                table: "RoutingOrderContacts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingOrderContacts_RoutingOrderId",
                table: "RoutingOrderContacts",
                column: "RoutingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingOrderContainers_RoutingOrderId",
                table: "RoutingOrderContainers",
                column: "RoutingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingOrderInvoices_RoutingOrderId",
                table: "RoutingOrderInvoices",
                column: "RoutingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingOrders_RoutingOrderNumber",
                table: "RoutingOrders",
                column: "RoutingOrderNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ROLineItems");

            migrationBuilder.DropTable(
                name: "RoutingOrderContacts");

            migrationBuilder.DropTable(
                name: "RoutingOrderContainers");

            migrationBuilder.DropTable(
                name: "RoutingOrderInvoices");

            migrationBuilder.DropTable(
                name: "RoutingOrders");
        }
    }
}
