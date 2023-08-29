using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Schema_Add_WeChat_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeChatOrWhatsApp",
                table: "POFulfillmentContacts",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeChatOrWhatsApp",
                table: "OrgContactPreferences",
                type: "NVARCHAR(32)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeChatOrWhatsApp",
                table: "POFulfillmentContacts");

            migrationBuilder.DropColumn(
                name: "WeChatOrWhatsApp",
                table: "OrgContactPreferences");
        }
    }
}
