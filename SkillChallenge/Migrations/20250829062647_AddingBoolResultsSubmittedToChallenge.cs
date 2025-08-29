using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AddingBoolResultsSubmittedToChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ResultsSubmitted",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 1,
                column: "ResultsSubmitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 2,
                column: "ResultsSubmitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 3,
                column: "ResultsSubmitted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 4,
                column: "ResultsSubmitted",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultsSubmitted",
                table: "Challenges");
        }
    }
}
