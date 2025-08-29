using SkillChallenge.Interfaces;
using SkillChallenge.Models;

// Ratinguppdatering med fler deltagare
// En users rating ska uppdateras jämfört både(?)
// de som har fler votes och de som har färre.
// Zeus fick 5 röster
// Oden fick 4 röster
// Adam fick 3 röster
// Berta fick 2 röster
// Cecar fick 1 röst
// Ta fram snittet av alla ratings utom din egen.
// För att ta fram din relativa position sorerar du rösterna ascending.
// Så den som fick flest röster hamnar sist i listan.
// Du jämför den positionen med antalet deltagare i challengen.
// Till exempel Zeuz har position 5 i en lista med 5 deltagare.
// 5 delat på 5 = 1.
// Så värdet 1 skickas med som actual outcome
// float rating_to_be_transfered = k_factor * (ACTUAL OUTCOME - user_win_chance);
// Oden fick 4 röster. 4/5 = 0.8
// Så nu behöver inte get_new_rating()-metoden en win del och en lose del.
// However, den som är på sista plats kommer alltså få 1/5 = 0.2.
// Men den som är sist ska ju ha 0. Och den som är först ska ha 1.
// Hur ska jag lösa det?
// 5/5 = 1
// 4/5 = 0.8
// 3/5 = 0.6
// 2/5 = 0.4
// 1/5 = 0.2
// I need five evend out values between 0 and 1.
// 1 divided by the number of participants minus 1!
// participantFactor = 1 / (actualParticipantCount - 1)
// Then take the index position of the users uploaded result in the uploadedResults list and multiply by the participantFactor
// participantFactor = 1 / (5 - 1) = 0.25
// Zeus index position is 4 in a list of 5 participants
// Zeus = 4 * 0.25 = 1
// Oden = 3 * 0.25 = 0.75
// Adam = 2 * 0.25 = 0.5
// Berta = 1 * 0.25 = 0.25
// Cecar = 0 * 0.25 = 0
// There you go.

// The k-factor could be adjusted mot antal deltagare.
// Fler deltagare, högre k-factor.
// ACTUAL OUTCOME kanske också can be adjusted based on the number of participants.
// Kan den vara mer än 1 och mindre än 0?
// Annars så kan alla deltagare lägga poäng i en pott (som ev baseras på k-factorn)
// och sedan fördelas den potten ut till deltagarna baserat på deras relativa position.




namespace SkillChallenge.Services
{
    public class EloRatingService
    {
        private readonly IRatingEntityRepository _ratingEntityRepository;

        int k_factor = 32;
        int c_value = 400;

        public EloRatingService(IRatingEntityRepository ratingEntityRepository)
        {
            _ratingEntityRepository = ratingEntityRepository;
        }

        // Ser till att alla users i challengen har en rating i den
        // subCategory som challengen har. Om usern inte har någon rating i den
        // subCategory så får den en ny rating med värdet 1000.
        public async Task EnsureRatingsExistForParticipantsAsync(IEnumerable<User> users, int categoryId, int subCategoryId, CancellationToken ct) // Can Challenge challenge be removed?
        {
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
                    user.CategoryRatingEntities.Add(newCategoryRating);
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
            // Sort uploaded results by votes ascending
            var uploadedResults = challenge.UploadedResults.OrderBy(ur => ur.Votes.Count).ToList();

            int actualPartcipantCount = users.Count();
            Console.WriteLine($"Actual participant count: {actualPartcipantCount}");

            float participantFactor = 1f / (actualPartcipantCount - 1);
            Console.WriteLine($"Participant factor: {participantFactor}");

            // Stores the new ratings before applying them in the foreach loop
            var newRatings = new Dictionary<SubCategoryRatingEntity, int>();

            int sumOfAllParticipantsRating = 0;
            // Get rating sum of all participants
            foreach (var user in users)
            {
                var userCategoryRatingEntity = user.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);
                var userSubCategoryRatingEntity = userCategoryRatingEntity.SubCategoryRatingEntities.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
                if (userSubCategoryRatingEntity == null) continue;
                int userRating = userSubCategoryRatingEntity.Rating;
                sumOfAllParticipantsRating += userRating;
            }

            foreach (var user in users)
            {
                // Find the index of the uploaded result that has this users user id
                // Find the uploaded result for this user
                var userResult = uploadedResults.FirstOrDefault(ur => ur.UserId == user.Id);
                if (userResult == null) continue;

                // The index of the users uploaded result in the uploadedResults list
                int position = uploadedResults.IndexOf(userResult);
                // OR
                //// The index of the uploadedResult that has the same number of votes as the userResult
                //int position = uploadedResults.FindIndex(ur => ur.Votes.Count == userResult.Votes.Count);

                //float userWinFactor = (float)position / actualPartcipantCount;
                float userWinFactor = (float)position * participantFactor;

                var categoryRatingEntity = user.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);
                var subCategoryRatingEntity = categoryRatingEntity.SubCategoryRatingEntities.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
                if (subCategoryRatingEntity == null) continue;
                int old_user_rating = subCategoryRatingEntity.Rating;

                sumOfAllParticipantsRating -= old_user_rating; // Remove self rating
                int opponentRatingAverage = (int)Math.Round((double)sumOfAllParticipantsRating / (actualPartcipantCount - 1));
                sumOfAllParticipantsRating += old_user_rating; // Put it back in for the next iteration

                float user_prevalue = get_prevalue(old_user_rating, c_value);
                float opponent_prevalue = get_prevalue(opponentRatingAverage, c_value);
                float user_win_chance = get_win_chance(user_prevalue, opponent_prevalue);
                int new_rating = get_new_rating(old_user_rating, k_factor, userWinFactor, user_win_chance);
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
        public static int get_new_rating(int old_rating, int k_factor, float winFactor, float user_win_chance)
        {
            float rating_to_be_transfered = k_factor * (winFactor - user_win_chance);
            float new_rating = old_rating + rating_to_be_transfered;
            int new_rating_as_int = (int)Math.Round(new_rating);
            return new_rating_as_int;
        }
    }
}
