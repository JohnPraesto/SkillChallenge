using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AnonymousVoting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "VoteEntities");

            // 1) Add ClientId as NULLABLE first (no default), then backfill, then make NOT NULL
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "VoteEntities",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true); // changed from nullable: false + defaultValue: ""

            migrationBuilder.AddColumn<string>(
                name: "IpHash",
                table: "VoteEntities",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgentHash",
                table: "VoteEntities",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            // 2) Provider-specific backfill
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(@"
                    UPDATE v
                    SET v.ClientId = LOWER(CONVERT(varchar(36), NEWID()))
                    FROM [dbo].[VoteEntities] v
                    WHERE v.ClientId IS NULL OR LTRIM(RTRIM(v.ClientId)) = '';
                ");
            }
            else if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql(@"
                    UPDATE VoteEntities
                    SET ClientId = lower(hex(randomblob(16)))
                    WHERE ClientId IS NULL OR trim(ClientId) = '';
                ");
            }

            // 3) Make ClientId NOT NULL (no default constraint)
            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "VoteEntities",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            // 4) Unique index
            migrationBuilder.CreateIndex(
                name: "IX_VoteEntities_ChallengeId_ClientId",
                table: "VoteEntities",
                columns: new[] { "ChallengeId", "ClientId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VoteEntities_ChallengeId_ClientId",
                table: "VoteEntities");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "VoteEntities");

            migrationBuilder.DropColumn(
                name: "IpHash",
                table: "VoteEntities");

            migrationBuilder.DropColumn(
                name: "UserAgentHash",
                table: "VoteEntities");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "VoteEntities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
