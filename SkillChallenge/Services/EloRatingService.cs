using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Services
{
    public class EloRatingService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRatingEntityRepository _ratingEntityRepository;

        int k_factor = 32;
        int c_value = 400;

        public EloRatingService(IUserRepository userRepository, IRatingEntityRepository ratingEntityRepository)
        {
            _userRepository = userRepository;
            _ratingEntityRepository = ratingEntityRepository;
        }

        // Ser till att alla users i challengen har en rating i den
        // subCategory som challengen har. Om usern inte har någon rating i den
        // subCategory så får den en ny rating med värdet 1000.
        public async Task EnsureRatingsExistForParticipantsAsync(IEnumerable<User> users, int categoryId, int subCategoryId, Challenge challenge, CancellationToken ct)
        {
            // Find the UploadedResults with the most votes
            int maxVotes = challenge.UploadedResults.Max(ur => ur.Votes.Count);
            var topResults = challenge.UploadedResults.Where(ur => ur.Votes.Count == maxVotes).ToList();

            foreach (var user in users)
            {
                var categoryRating = user.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);

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
                    var hasSubCategory = categoryRating.SubCategoryRatingEntities.Any(s => s.SubCategoryId == subCategoryId);

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

        public async Task UpdateEloRatingsAsync(IEnumerable<User> users, int categoryId, int subCategoryId, Challenge challenge, CancellationToken ct)
        {
            // Find the UploadedResults with the most votes
            int maxVotes = challenge.UploadedResults.Max(ur => ur.Votes.Count);
            var topResults = challenge.UploadedResults.Where(ur => ur.Votes.Count == maxVotes).ToList();

            // Stores the new ratings before applying them in the foreach loop
            var newRatings = new Dictionary<SubCategoryRatingEntity, int>();
            // This foreach loop takes a users rating and averages all
            // opponent ratings into one rating.
            foreach (var user in users)
            {
                // Get user rating
                int opponentRatingSum = 0;
                bool userWon = topResults.Any(r => r.UserId == user.Id);
                var categoryRatingEntity = user.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);
                var subCategoryRatingEntity = categoryRatingEntity.SubCategoryRatingEntities.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
                if (subCategoryRatingEntity == null) continue;
                int old_user_rating = subCategoryRatingEntity.Rating;

                // Get all opponent ratings
                foreach (var opponent in users)
                {
                    var opponentCategoryRatingEntity = opponent.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);
                    var opponentSubCategoryRatingEntity = opponentCategoryRatingEntity.SubCategoryRatingEntities.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
                    if (opponentSubCategoryRatingEntity == null) continue;
                    int opponent_rating = opponentSubCategoryRatingEntity.Rating;
                    opponentRatingSum += opponent_rating;
                }
                opponentRatingSum -= old_user_rating; // Remove self rating
                int opponent_rating_average = (int)Math.Round((double)opponentRatingSum / (users.Count() - 1)); // Average opponent ratings

                float user_prevalue = get_prevalue(old_user_rating, c_value);
                float opponent_prevalue = get_prevalue(opponent_rating_average, c_value);
                float user_win_chance = get_win_chance(user_prevalue, opponent_prevalue);
                int new_rating = get_new_rating(old_user_rating, k_factor, userWon, user_win_chance);
                newRatings[subCategoryRatingEntity] = new_rating;
            }

            //Apply all the stored new ratings
            foreach (var kvp in newRatings)
            {
                await _ratingEntityRepository.SetNewSubCategoryRatingAsync(kvp.Key, kvp.Value, ct);
            }
        }

        /* 
         R-a = Ra + K*(Sa — Ea)

        R-a represents the new, updated, Elo rating of the player after the match, 
        Ra represents the Elo rating that the player had before the match, 
        Ea denotes the expected outcome of the match and 
        SA is the actual outcome of the match.
        Variable K denotes a scaling factor that determines how much influence
        each particular match can have on the overall Elo rating of the player. 

         in the case of victory Sa = 1
         in the case of loss, Sa = 0
         and in the case of a tie Sa = 0.5.

         Ea = Qa /(Qa + Qb), where Qa = 10^(Ra/c) and Qb = 10^(Rb/c).
         */

        public static float get_prevalue(int old_rating, int c_value)
        {
            float prevalue = MathF.Pow(10, (float)old_rating / c_value);
            return prevalue;
        }

        public static float get_win_chance(float user_prevalue, float opponent_prevalue)
        {
            return user_prevalue / (user_prevalue + opponent_prevalue);
        }
        // DRAWS NOT SET UP YET
        public static int get_new_rating(int old_rating, int k_factor, bool win, float user_win_chance)
        {
            if (win)
            {
                float rating_to_be_transfered = k_factor * (1 - user_win_chance);
                float new_rating = old_rating + rating_to_be_transfered;
                int new_rating_as_int = (int)Math.Round(new_rating);
                return new_rating_as_int;
            }
            else
            {

                float rating_to_be_transfered = k_factor * (0 - user_win_chance);
                float new_rating = old_rating + rating_to_be_transfered;
                int new_rating_as_int = (int)Math.Round(new_rating);
                return new_rating_as_int;
            }
        }
    }
}
