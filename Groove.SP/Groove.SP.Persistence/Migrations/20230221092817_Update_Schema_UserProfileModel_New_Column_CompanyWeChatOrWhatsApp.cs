using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_UserProfileModel_New_Column_CompanyWeChatOrWhatsApp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyWeChatOrWhatsApp",
                table: "UserProfiles",
                type: "VARCHAR(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "label.companyWeChatIDOrWhatsApp", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Company WeChatID/WhatsApp", null, "Company WeChatID/WhatsApp (SC)", "Company WeChatID/WhatsApp (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.companyWeChatIDOrWhatsApp");

            migrationBuilder.DropColumn(
                name: "CompanyWeChatOrWhatsApp",
                table: "UserProfiles");
        }
    }
}
