using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Add_Translations_MixedCarton : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "label.mixedCarton?", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mixed Carton?", null, "Mixed Carton? (SC)", "Mixed Carton? (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "validation.totalBookedPackageMustGreaterThanZero", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Total quantity of Booked Package must be greater than 0.", null, "Total quantity of Booked Package must be greater than 0. (SC)", "Total quantity of Booked Package must be greater than 0. (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.mixedCarton?");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "validation.totalBookedPackageMustGreaterThanZero");
        }
    }
}
