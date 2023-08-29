using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Schema_Add_Invoices_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfSubmissionToCruise",
                table: "Invoices",
                type: "DATETIME2(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDueDate",
                table: "Invoices",
                type: "DATETIME2(7)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfSubmissionToCruise",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentDueDate",
                table: "Invoices");
        }
    }
}
