using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingRatingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RatingEntities");

            migrationBuilder.CreateTable(
                name: "CategoryRatingEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryRatingEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryRatingEntities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryRatingEntities_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryRatingEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CategoryRatingEntityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryRatingEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategoryRatingEntities_CategoryRatingEntities_CategoryRatingEntityId",
                        column: x => x.CategoryRatingEntityId,
                        principalTable: "CategoryRatingEntities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubCategoryRatingEntities_SubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "SubCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRatingEntities_CategoryId",
                table: "CategoryRatingEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRatingEntities_UserId",
                table: "CategoryRatingEntities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryRatingEntities_CategoryRatingEntityId",
                table: "SubCategoryRatingEntities",
                column: "CategoryRatingEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryRatingEntities_SubCategoryId",
                table: "SubCategoryRatingEntities",
                column: "SubCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubCategoryRatingEntities");

            migrationBuilder.DropTable(
                name: "CategoryRatingEntities");

            migrationBuilder.CreateTable(
                name: "RatingEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatingEntities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RatingEntities_UserId",
                table: "RatingEntities",
                column: "UserId");
        }
    }
}
