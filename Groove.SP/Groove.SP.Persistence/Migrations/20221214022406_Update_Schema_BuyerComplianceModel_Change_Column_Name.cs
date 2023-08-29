using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_BuyerComplianceModel_Change_Column_Name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAllowUnrecognizedPO",
                table: "BuyerCompliances",
                newName: "IsAllowMissingPO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAllowMissingPO",
                table: "BuyerCompliances",
                newName: "IsAllowUnrecognizedPO");
        }
    }
}
