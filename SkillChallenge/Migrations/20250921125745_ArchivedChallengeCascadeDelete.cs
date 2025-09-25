using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class ArchivedChallengeCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchivedChallengeUsers_ArchivedChallenges_ArchivedChallengeId",
                table: "ArchivedChallengeUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivedChallengeUsers_ArchivedChallenges_ArchivedChallengeId",
                table: "ArchivedChallengeUsers",
                column: "ArchivedChallengeId",
                principalTable: "ArchivedChallenges",
                principalColumn: "ArchivedChallengeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchivedChallengeUsers_ArchivedChallenges_ArchivedChallengeId",
                table: "ArchivedChallengeUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivedChallengeUsers_ArchivedChallenges_ArchivedChallengeId",
                table: "ArchivedChallengeUsers",
                column: "ArchivedChallengeId",
                principalTable: "ArchivedChallenges",
                principalColumn: "ArchivedChallengeId");
        }
    }
}
