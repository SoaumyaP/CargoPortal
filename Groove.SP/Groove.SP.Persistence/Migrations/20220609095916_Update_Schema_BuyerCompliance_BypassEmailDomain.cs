using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_BuyerCompliance_BypassEmailDomain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BypassEmailDomain",
                table: "BuyerCompliances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.emailDomainToBypassApproval",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Email Domain To Bypass Approval", "Email Domain To Bypass Approval (SC)", "Email Domain To Bypass Approval (TC)" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[] { "tooltip.defaultSendToCustomer", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Default send to Customer.", null, "Default send to Customer. (SC)", "Default send to Customer. (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "tooltip.defaultSendToCustomer");

            migrationBuilder.DropColumn(
                name: "BypassEmailDomain",
                table: "BuyerCompliances");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.emailDomainToBypassApproval",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Email Domain To Bypass The Approval", "Email Domain To Bypass The Approval (SC)", "Email Domain To Bypass The Approval (TC)" });
        }
    }
}
