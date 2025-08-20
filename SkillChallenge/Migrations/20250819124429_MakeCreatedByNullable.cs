using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class MakeCreatedByNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_AspNetUsers_CreatedBy",
                table: "Challenges");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Challenges",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_AspNetUsers_CreatedBy",
                table: "Challenges",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_AspNetUsers_CreatedBy",
                table: "Challenges");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Challenges",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_AspNetUsers_CreatedBy",
                table: "Challenges",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
