using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Schema_Edit_Name_IsUnrecognizedPO_Field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsUnrecognizedPO",
                table: "BuyerCompliances",
                newName: "IsAllowUnrecognizedPO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAllowUnrecognizedPO",
                table: "BuyerCompliances",
                newName: "IsUnrecognizedPO");
        }
    }
}
