using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Label_companyWeChatIDOrWhatsApp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.companyWeChatIDOrWhatsApp",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Company WeChat ID/WhatsApp", "Company WeChat ID/WhatsApp (SC)", "Company WeChat ID/WhatsApp (TC)" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.companyWeChatIDOrWhatsApp",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Company WeChatID/WhatsApp", "Company WeChatID/WhatsApp (SC)", "Company WeChatID/WhatsApp (TC)" });
        }
    }
}
