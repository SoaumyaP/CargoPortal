using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.CSFE.Persistence.Migrations
{
    public partial class Schema_Add_UserOffices_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaxpayerId",
                table: "Organizations",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserOffices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<long>(type: "bigint", nullable: false),
                    CorpMarketingContactName = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    CorpMarketingContactEmail = table.Column<string>(type: "varchar(128)", nullable: true),
                    OPManagementContactName = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    OPManagementContactEmail = table.Column<string>(type: "varchar(128)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOffices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOffices_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOffices_LocationId",
                table: "UserOffices",
                column: "LocationId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOffices");

            migrationBuilder.DropColumn(
                name: "TaxpayerId",
                table: "Organizations");
        }
    }
}
