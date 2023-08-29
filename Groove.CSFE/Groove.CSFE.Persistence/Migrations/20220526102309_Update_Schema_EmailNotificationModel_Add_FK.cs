using Microsoft.EntityFrameworkCore.Migrations;

namespace Groove.CSFE.Persistence.Migrations
{
    public partial class Update_Schema_EmailNotificationModel_Add_FK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EmailNotifications",
                type: "NVARCHAR(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(255)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotifications_CountryId",
                table: "EmailNotifications",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotifications_CustomerId",
                table: "EmailNotifications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotifications_OrganizationId",
                table: "EmailNotifications",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotifications_Countries_CountryId",
                table: "EmailNotifications",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotifications_Organizations_CustomerId",
                table: "EmailNotifications",
                column: "CustomerId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotifications_Organizations_OrganizationId",
                table: "EmailNotifications",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotifications_Countries_CountryId",
                table: "EmailNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotifications_Organizations_CustomerId",
                table: "EmailNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotifications_Organizations_OrganizationId",
                table: "EmailNotifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotifications_CountryId",
                table: "EmailNotifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotifications_CustomerId",
                table: "EmailNotifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotifications_OrganizationId",
                table: "EmailNotifications");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EmailNotifications",
                type: "NVARCHAR(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(255)");
        }
    }
}
