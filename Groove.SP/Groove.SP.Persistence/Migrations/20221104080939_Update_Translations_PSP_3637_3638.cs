using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Translations_PSP_3637_3638 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.thankYou",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Thank you", "Thank you (SC)", "Thank you (TC)" });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "msg.linkToSurveyIsNoLongerActive",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Oops! The link you clicked is no longer active.", "Oops! The link you clicked is no longer active. (SC)", "Oops! The link you clicked is no longer active. (TC)" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "English", "Note", "SimplifiedChinese", "TraditionalChinese", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { "label.number", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "No.", null, "No. (SC)", "No. (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "msg.sorryMissYouOnThisSurvey", "System", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "We love to hear your opinions. Sorry, we missed you on this activity.", null, "We love to hear your opinions. Sorry, we missed you on this activity. (SC)", "We love to hear your opinions. Sorry, we missed you on this activity. (TC)", null, new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.number");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "msg.sorryMissYouOnThisSurvey");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "label.thankYou",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Thank You", "Thank You (SC)", "Thank You (TC)" });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Key",
                keyValue: "msg.linkToSurveyIsNoLongerActive",
                columns: new[] { "English", "SimplifiedChinese", "TraditionalChinese" },
                values: new object[] { "Oops! The link you clicked is no longer active. We love to hear your opinions. Sorry, we missed you on this activity.", "Oops! The link you clicked is no longer active. We love to hear your opinions. Sorry, we missed you on this activity. (SC)", "Oops! The link you clicked is no longer active. We love to hear your opinions. Sorry, we missed you on this activity. (TC)" });
        }
    }
}
