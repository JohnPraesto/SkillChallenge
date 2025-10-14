using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnEndDate",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnVotingEnd",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyTwoDaysBeforeEndDate",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-123",
                columns: new[] { "NotifyOnEndDate", "NotifyOnVotingEnd", "NotifyTwoDaysBeforeEndDate" },
                values: new object[] { true, true, true });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-456",
                columns: new[] { "NotifyOnEndDate", "NotifyOnVotingEnd", "NotifyTwoDaysBeforeEndDate" },
                values: new object[] { true, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyOnEndDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NotifyOnVotingEnd",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NotifyTwoDaysBeforeEndDate",
                table: "AspNetUsers");
        }
    }
}
