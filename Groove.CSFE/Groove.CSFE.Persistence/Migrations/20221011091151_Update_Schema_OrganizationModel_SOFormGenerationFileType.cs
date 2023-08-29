using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.CSFE.Persistence.Migrations
{
    public partial class Update_Schema_OrganizationModel_SOFormGenerationFileType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SOFormGenerationFileType",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 10);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SOFormGenerationFileType",
                table: "Organizations");
        }
    }
}
