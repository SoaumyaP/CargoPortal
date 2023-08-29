using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.CSFE.Persistence.Migrations
{
    public partial class Update_Schema_OrganizationModel_New_Column_WeChatOrWhatsApp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeChatOrWhatsApp",
                table: "Organizations",
                type: "varchar(32)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeChatOrWhatsApp",
                table: "Organizations");
        }
    }
}
