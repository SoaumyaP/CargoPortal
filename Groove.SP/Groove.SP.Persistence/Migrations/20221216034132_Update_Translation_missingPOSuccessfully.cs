using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Translation_missingPOSuccessfully : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "save.missingPOSuccessfully",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Request for missing PO has been sent successfully", "Request for missing PO has been sent successfully (SC)", "Request for missing PO has been sent successfully (TC)" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "save.missingPOSuccessfully",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Request for unauthorize PO has been sent successfully", "Request for unauthorize PO has been sent successfully (SC)", "Request for unauthorize PO has been sent successfully (TC)" });
        }
    }
}
