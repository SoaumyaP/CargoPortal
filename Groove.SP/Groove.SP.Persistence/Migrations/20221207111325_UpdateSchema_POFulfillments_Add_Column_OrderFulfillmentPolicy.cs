using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class UpdateSchema_POFulfillments_Add_Column_OrderFulfillmentPolicy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderFulfillmentPolicy",
                table: "POFulfillments",
                type: "INT",
                nullable: false,
                defaultValue: 20);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderFulfillmentPolicy",
                table: "POFulfillments");
        }
    }
}
