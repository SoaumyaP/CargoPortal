using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class New_Schema_ViewSettingModel_ViewRoleSettingModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewSettings",
                columns: table => new
                {
                    ViewId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Field = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR(256)", maxLength: 256, nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: true),
                    ModuleId = table.Column<string>(type: "NVARCHAR(256)", maxLength: 256, nullable: false),
                    ViewType = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewSettings", x => x.ViewId);
                });

            migrationBuilder.CreateTable(
                name: "ViewRoleSettings",
                columns: table => new
                {
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ViewId = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewRoleSettings", x => new { x.RoleId, x.ViewId });
                    table.ForeignKey(
                        name: "FK_ViewRoleSettings_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ViewRoleSettings_ViewSettings_ViewId",
                        column: x => x.ViewId,
                        principalTable: "ViewSettings",
                        principalColumn: "ViewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewRoleSettings_ViewId",
                table: "ViewRoleSettings",
                column: "ViewId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewRoleSettings");

            migrationBuilder.DropTable(
                name: "ViewSettings");
        }
    }
}
