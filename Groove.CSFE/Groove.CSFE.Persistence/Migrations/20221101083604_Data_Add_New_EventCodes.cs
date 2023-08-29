using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.CSFE.Persistence.Migrations
{
    public partial class Data_Add_New_EventCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EventCodes",
                columns: new[] { "ActivityCode", "ActivityDescription", "ActivityTypeCode", "CreatedBy", "CreatedDate", "LocationRequired", "RemarkRequired", "SortSequence", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "2055", "Payment received", "SA", "System", new DateTime(2021, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), false, false, 641L, null, null });

            migrationBuilder.InsertData(
                table: "EventCodes",
                columns: new[] { "ActivityCode", "ActivityDescription", "ActivityTypeCode", "CreatedBy", "CreatedDate", "LocationRequired", "RemarkRequired", "SortSequence", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "3052", "Container roll over", "CA", "System", new DateTime(2021, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), false, false, 541L, null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EventCodes",
                keyColumn: "ActivityCode",
                keyValue: "2055");

            migrationBuilder.DeleteData(
                table: "EventCodes",
                keyColumn: "ActivityCode",
                keyValue: "3052");
        }
    }
}
