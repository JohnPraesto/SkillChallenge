using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillChallenge.Data;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;

namespace SkillChallenge.Controllers
{
    [Route("api/leaderboard")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly AppDbContext _context;

        public LeaderboardController(ICategoryRepository categoryRepo, AppDbContext context)
        {
            _categoryRepo = categoryRepo;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard(CancellationToken ct)
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync(ct);

            var leaderboard = new List<LeaderboardCategoryDTO>();

            foreach (var category in categories)
            {
                var subCategoryDTOs = new List<LeaderboardSubCategoryDTO>();

                foreach (var sub in category.SubCategories)
                {
                    var topUsers = await _context.SubCategoryRatingEntities
                        .Where(scr => scr.SubCategoryId == sub.SubCategoryId)
                        .Include(scr => scr.User)
                        .OrderByDescending(scr => scr.Rating)
                        .Take(10)
                        .Select(scr => new LeaderboardUserDTO
                        {
                            UserId = scr.UserId,
                            UserName = scr.User.UserName,
                            ProfilePicture = scr.User.ProfilePicture,
                            Rating = scr.Rating
                        })
                        .ToListAsync(ct);

                    subCategoryDTOs.Add(new LeaderboardSubCategoryDTO
                    {
                        SubCategoryId = sub.SubCategoryId,
                        SubCategoryName = sub.SubCategoryName,
                        ImagePath = sub.ImagePath,
                        TopUsers = topUsers
                    });
                }

                leaderboard.Add(new LeaderboardCategoryDTO
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    SubCategories = subCategoryDTOs
                });
            }

            return Ok(leaderboard);
        }
    }
}
