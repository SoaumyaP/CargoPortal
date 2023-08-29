using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Schema_Add_New_Factory_Role : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Activated", "CreatedBy", "CreatedDate", "Description", "IsInternal", "IsOfficial", "Name", "Status", "UpdatedBy", "UpdatedDate" },
                values: new object[] { 13L, false, "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, true, "Factory", 1, null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 13L);
        }
    }
}
