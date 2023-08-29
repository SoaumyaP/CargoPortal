using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Schema_Add_MobileApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mobile");

            migrationBuilder.CreateTable(
                name: "Applications",
                schema: "mobile",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<string>(type: "nvarchar(52)", maxLength: 52, nullable: false),
                    PublishedDate = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsDiscontinued = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PackageName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PackageUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_PublishedDate",
                schema: "mobile",
                table: "Applications",
                column: "PublishedDate")
                .Annotation("SqlServer:Include", new[] { "Version", "PackageUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Version",
                schema: "mobile",
                table: "Applications",
                column: "Version",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "IsDiscontinued" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications",
                schema: "mobile");
        }
    }
}
