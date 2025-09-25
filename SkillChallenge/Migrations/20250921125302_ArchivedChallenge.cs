using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class ArchivedChallengeMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchivedChallenges",
                columns: table => new
                {
                    ArchivedChallengeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    ChallengeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedChallenges", x => x.ArchivedChallengeId);
                });

            migrationBuilder.CreateTable(
                name: "ArchivedChallengeUsers",
                columns: table => new
                {
                    ArchivedChallengeUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RatingChange = table.Column<int>(type: "int", nullable: false),
                    Placement = table.Column<int>(type: "int", nullable: false),
                    ArchivedChallengeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedChallengeUsers", x => x.ArchivedChallengeUserId);
                    table.ForeignKey(
                        name: "FK_ArchivedChallengeUsers_ArchivedChallenges_ArchivedChallengeId",
                        column: x => x.ArchivedChallengeId,
                        principalTable: "ArchivedChallenges",
                        principalColumn: "ArchivedChallengeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedChallengeUsers_ArchivedChallengeId",
                table: "ArchivedChallengeUsers",
                column: "ArchivedChallengeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivedChallengeUsers");

            migrationBuilder.DropTable(
                name: "ArchivedChallenges");
        }
    }
}
