using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_BookingTimelessModel_AirBookingPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AirEarlyBookingTimeless",
                table: "BookingTimelesses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AirLateBookingTimeless",
                table: "BookingTimelesses",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "label.airEarlyBookingPeriod", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Air Early Booking Period", null, "Air Early Booking Period (SC)", "Air Early Booking Period (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "label.airLateBookingPeriod", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Air Late Booking Period", null, "Air Late Booking Period (SC)", "Air Late Booking Period (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.airEarlyBookingPeriod");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.airLateBookingPeriod");

            migrationBuilder.DropColumn(
                name: "AirEarlyBookingTimeless",
                table: "BookingTimelesses");

            migrationBuilder.DropColumn(
                name: "AirLateBookingTimeless",
                table: "BookingTimelesses");
        }
    }
}
