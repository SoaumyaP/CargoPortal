using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Translation_Update_termsAndInstructions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.termsAndInstructions",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Terms and Instructions", "Terms and Instructions (SC)", "Terms and Instructions (TC)" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.termsAndInstructions",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Terms And Instructions", "Terms And Instructions (SC)", "Terms And Instructions (TC)" });
        }
    }
}
