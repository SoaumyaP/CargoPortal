using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Translation_Update_BalanceOfGoods_5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.warehouseTransactionTitle",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Warehouse {{warehouseCode}} of {{principleCode}}", "Warehouse {{warehouseCode}} of {{principleCode}} (SC)", "Warehouse {{warehouseCode}} of {{principleCode}} (TC)" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.warehouseTransactionTitle",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Warehouse {{articleName}} of {{principleCode}}", "Warehouse {{articleName}} of {{principleCode}} (SC)", "Warehouse {{articleName}} of {{principleCode}} (TC)" });
        }
    }
}
