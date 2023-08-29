using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class New_Translations_ATD_Range_Validation_Error_Message : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "validation.dateRangeToRunReport",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Please select either ETD, ETA, ATD or Ex-work Date range as the report criteria.", "Please select either ETD, ETA, ATD or Ex-work Date range as the report criteria.(SC)", "Please select either ETD, ETA, ATD or Ex-work Date range as the report criteria.(TC)" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "validation.atdDurationInvalid", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ATD To and From must be within 2 months", null, "ATD To and From must be within 2 months (SC)", "ATD To and From must be within 2 months (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "validation.atdRangeInvalid", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ATD To must be greater than ATD From", null, "ATD To must be greater than ATD From (SC)", "ATD To must be greater than ATD From (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "validation.atdDurationInvalid");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "validation.atdRangeInvalid");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "validation.dateRangeToRunReport",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Please select either ETD, ETA or Ex-work Date range as the report criteria.", "Please select either ETD, ETA or Ex-work Date range as the report criteria.(SC)", "Please select either ETD, ETA or Ex-work Date range as the report criteria.(TC)" });
        }
    }
}
