using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class AdminCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-123",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "UserName" },
                values: new object[] { "john_praesto@hotmail.com", "JOHN_PRAESTO@HOTMAIL.COM", "JOHN", "AQAAAAIAAYagAAAAEKQDRjO0QissdKacQsQZ2Y1zSJc3ZLNbEbCSfwKwBAX0WOoLI0gqJo/T76Yu6SvbDg==", "john" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-123",
                columns: new[] { "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "UserName" },
                values: new object[] { "admin@skillchallenge.com", "ADMIN@SKILLCHALLENGE.COM", "ADMIN", "AQAAAAIAAYagAAAAEIzZ1ipYa+9PoN6PNCJektB+44UdZJWEv/RnJtum84hmALg1Z4Gl5h9C0nDM2CIXOw==", "admin" });
        }
    }
}
