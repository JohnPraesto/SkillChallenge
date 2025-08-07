using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Services
{
    public class EloRatingService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRatingEntityRepository _ratingEntityRepository;

        public EloRatingService(IUserRepository userRepository, IRatingEntityRepository ratingEntityRepository)
        {
            _userRepository = userRepository;
            _ratingEntityRepository = ratingEntityRepository;
        }

        // Ser till att alla users i challengen har en rating i den
        // subCategory som challengen har. Om usern inte har någon rating i den
        // subCategory så får den en ny rating med värdet 1000.

        // Så, efter att vi säkrat att alla deltagare har en rating.
        // Då är det väl dags att göra elo-beräkningarna?
        public async Task EnsureRatingsForParticipantsAsync(IEnumerable<User> users, int categoryId, int subCategoryId, CancellationToken ct) // INTE CHALLENGE ID?
        {
            foreach (var user in users)
            {
                var categoryRating = user.CategoryRatingEntities
                    .FirstOrDefault(c => c.CategoryId == categoryId);

                if (categoryRating == null)
                {
                    var newCategoryRating = new CategoryRatingEntity
                    {
                        CategoryId = categoryId,
                        UserId = user.Id,
                        SubCategoryRatingEntities = new List<SubCategoryRatingEntity>
                        {
                            new SubCategoryRatingEntity
                            {
                                SubCategoryId = subCategoryId,
                                Rating = 1000
                            }
                        }
                    };
                    await _ratingEntityRepository.AddAsync(newCategoryRating, ct);
                }
                else
                {
                    var hasSubCategory = categoryRating.SubCategoryRatingEntities
                        .Any(s => s.SubCategoryId == subCategoryId);

                    if (!hasSubCategory)
                    {
                        categoryRating.SubCategoryRatingEntities.Add(new SubCategoryRatingEntity
                        {
                            SubCategoryId = subCategoryId,
                            Rating = 1000
                        });
                        await _ratingEntityRepository.AddAsync(categoryRating, ct);
                    }
                }
            }
        }

        public static void meme()
        {
            //def get_expected_prevalue(old_rating, c_value):
            //return 10 * *(old_rating / c_value)

            //def get_win_chance(fighter_prevalue, other_fighter_prevalue):
            //    return fighter_prevalue / (fighter_prevalue + other_fighter_prevalue)

            //def set_new_rating(old_rating, k_value, outcome_value, win_chance):
            //    rating_to_be_transfered = k_value * (outcome_value - win_chance)
            //    new_rating = old_rating + rating_to_be_transfered
            //    return new_rating
        }
    }
}
