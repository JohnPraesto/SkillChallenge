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

        // Så, efter att vi säkrat att alla deltagare har en rating.
        // Då är det väl dags att göra elo-beräkningarna?

        // Och nu när det typ är satt (ej kollat om det ens funkar)
        // Så är nästa steg att hitta ett sätt att räkna rösterna
        public async Task EnsureRatingsForParticipantsAsync(IEnumerable<User> users, int categoryId, int subCategoryId, CancellationToken ct)
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
            // This foreach loop takes a users rating and averages all
            // opponent ratings into one rating.
            foreach (var user in users)
            {
                // Get user rating
                int opponent_rating_sum = 0;
                var categoryRatingEntity = user.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);
                var subCategoryRatingEntity = categoryRatingEntity.SubCategoryRatingEntities.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
                int old_user_rating = subCategoryRatingEntity.Rating;

                // Get all opponent ratings
                foreach (var opponent in users)
                {
                    var opponentCategoryRatingEntity = opponent.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);
                    var opponentSubCategoryRatingEntity = opponentCategoryRatingEntity.SubCategoryRatingEntities.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
                    int opponent_rating = opponentSubCategoryRatingEntity.Rating;
                    opponent_rating_sum += opponent_rating;
                }
                opponent_rating_sum -= old_user_rating; // Remove self rating
                int opponent_rating_average = (int)Math.Round((double)opponent_rating_sum / (users.Count() - 1)); // Average opponent ratings

                float user_prevalue = get_prevalue(old_user_rating, c_value);
                float opponent_prevalue = get_prevalue(opponent_rating_average, c_value);
                float user_win_chance = get_win_chance(user_prevalue, opponent_prevalue);
                int new_rating = set_new_rating(old_user_rating, k_factor, true, user_win_chance); // WHO GOT THE MOST VOTES ARE NOT YET SOLVED
                await _ratingEntityRepository.SetNewSubCategoryRatingAsync(subCategoryRatingEntity, new_rating, ct); // kanske är nåt med att subCategories måste .Includas för att EF ska tracka den... får se
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
        public static int set_new_rating(int old_rating, int k_factor, bool win, float user_win_chance)
        {
            if (win)
            {
                int user_win_chance_as_int = (int)Math.Round(user_win_chance);
                int rating_to_be_transfered = k_factor * (1 - user_win_chance_as_int);
                int new_rating = old_rating + rating_to_be_transfered;
                return new_rating;
            }
            else
            {
                int user_win_chance_as_int = (int)Math.Round(user_win_chance);
                int rating_to_be_transfered = k_factor * (0 - user_win_chance_as_int);
                int new_rating = old_rating + rating_to_be_transfered;
                return new_rating;
            }
        }
    }
}
