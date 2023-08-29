using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_ArticleMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "ArticleMaster",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ArticleMaster",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleMaster_Id",
                table: "ArticleMaster",
                column: "Id",
                unique: true)
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "CompanyCode", "CompanyType", "PONo", "ItemNo", "ShipmentNo", "POSeq", "DestCode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArticleMaster_Id",
                table: "ArticleMaster");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ArticleMaster");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ArticleMaster");
        }
    }
}
