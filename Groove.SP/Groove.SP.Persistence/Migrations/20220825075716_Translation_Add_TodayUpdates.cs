using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Translation_Add_TodayUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "label.todayUpdate.currentMobileReleased", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Download {{appPackageLink}} application ({{appVersion}}) here", null, "Download {{appPackageLink}} application ({{appVersion}}) here (SC)", "Download {{appPackageLink}} application ({{appVersion}}) here (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "label.todayUpdate.newMobileReleased", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A new version ({{appVersion}}) of {{appPackageLink}} application is available", null, "A new version ({{appVersion}}) of {{appPackageLink}} application is available (SC)", "A new version ({{appVersion}}) of {{appPackageLink}} application is available (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "label.warehouseReceive", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Warehouse Receive", null, "Warehouse Receive (SC)", "Warehouse Receive (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.todayUpdate.currentMobileReleased");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.todayUpdate.newMobileReleased");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.warehouseReceive");
        }
    }
}
