using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AddingNumberOfParticipantsToChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengeUsers_AspNetUsers_UsersId",
                table: "ChallengeUsers");

            migrationBuilder.RenameColumn(
                name: "UsersId",
                table: "ChallengeUsers",
                newName: "ParticipantsId");

            migrationBuilder.RenameIndex(
                name: "IX_ChallengeUsers_UsersId",
                table: "ChallengeUsers",
                newName: "IX_ChallengeUsers_ParticipantsId");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfParticipants",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 1,
                column: "NumberOfParticipants",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 2,
                column: "NumberOfParticipants",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 3,
                column: "NumberOfParticipants",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "ChallengeId",
                keyValue: 4,
                column: "NumberOfParticipants",
                value: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeUsers_AspNetUsers_ParticipantsId",
                table: "ChallengeUsers",
                column: "ParticipantsId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengeUsers_AspNetUsers_ParticipantsId",
                table: "ChallengeUsers");

            migrationBuilder.DropColumn(
                name: "NumberOfParticipants",
                table: "Challenges");

            migrationBuilder.RenameColumn(
                name: "ParticipantsId",
                table: "ChallengeUsers",
                newName: "UsersId");

            migrationBuilder.RenameIndex(
                name: "IX_ChallengeUsers_ParticipantsId",
                table: "ChallengeUsers",
                newName: "IX_ChallengeUsers_UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeUsers_AspNetUsers_UsersId",
                table: "ChallengeUsers",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
