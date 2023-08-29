using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groove.SP.Persistence.Migrations
{
    public partial class Update_Schema_SurveyAnswer_Add_FK_OptionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SurveyId",
                table: "SurveyAnswers");

            migrationBuilder.AddColumn<long>(
                name: "OptionId",
                table: "SurveyAnswers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyAnswers_OptionId",
                table: "SurveyAnswers",
                column: "OptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyAnswers_SurveyQuestionOptions_OptionId",
                table: "SurveyAnswers",
                column: "OptionId",
                principalTable: "SurveyQuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyAnswers_SurveyQuestionOptions_OptionId",
                table: "SurveyAnswers");

            migrationBuilder.DropIndex(
                name: "IX_SurveyAnswers_OptionId",
                table: "SurveyAnswers");

            migrationBuilder.DropColumn(
                name: "OptionId",
                table: "SurveyAnswers");

            migrationBuilder.AddColumn<long>(
                name: "SurveyId",
                table: "SurveyAnswers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
