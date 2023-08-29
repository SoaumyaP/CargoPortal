using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.CSFE.Persistence.Migrations
{
    public partial class Update_Schema_EventCodeModel_New_Column_Status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EventCodes",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "EventCodes");
        }
    }
}
