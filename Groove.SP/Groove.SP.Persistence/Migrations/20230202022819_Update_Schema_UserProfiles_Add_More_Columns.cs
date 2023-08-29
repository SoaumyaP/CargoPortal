using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_UserProfiles_Add_More_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyAddressLine1",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddressLine2",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddressLine3",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddressLine4",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OPContactEmail",
                table: "UserProfiles",
                type: "NVARCHAR(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OPContactName",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OPCountryId",
                table: "UserProfiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OPLocationName",
                table: "UserProfiles",
                type: "NVARCHAR(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxpayerId",
                table: "UserProfiles",
                type: "NVARCHAR(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { "label.companyAddress", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Company Address", null, "Company Address (SC)", "Company Address (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "label.operationContactCity", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Operation Contact City", null, "Operation Contact City (SC)", "Operation Contact City (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "label.operationContactCountry", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Operation Contact Country", null, "Operation Contact Country (SC)", "Operation Contact Country (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "label.operationContactEmail", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Operation Contact Email", null, "Operation Contact Email (SC)", "Operation Contact Email (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "label.operationContactName", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Operation Contact Name", null, "Operation Contact Name (SC)", "Operation Contact Name (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "label.taxpayerId", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Taxpayer ID", null, "Taxpayer ID (SC)", "Taxpayer ID (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.companyAddress");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.operationContactCity");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.operationContactCountry");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.operationContactEmail");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.operationContactName");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.taxpayerId");

            migrationBuilder.DropColumn(
                name: "CompanyAddressLine1",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CompanyAddressLine2",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CompanyAddressLine3",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CompanyAddressLine4",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OPContactEmail",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OPContactName",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OPCountryId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OPLocationName",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "TaxpayerId",
                table: "UserProfiles");
        }
    }
}
