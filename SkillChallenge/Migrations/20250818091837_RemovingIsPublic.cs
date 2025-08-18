using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class RemovingIsPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Challenges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 1,
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 2,
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 3,
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 4,
                column: "IsPublic",
                value: true);
        }
    }
}
