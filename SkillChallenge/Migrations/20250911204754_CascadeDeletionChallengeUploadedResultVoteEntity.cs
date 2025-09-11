using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillChallenge.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeletionChallengeUploadedResultVoteEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "CategoryEntityRatingId",
            //    table: "SubCategoryRatingEntities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryEntityRatingId",
                table: "SubCategoryRatingEntities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
