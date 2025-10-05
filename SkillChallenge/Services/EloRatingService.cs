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
// Så värdet 1 skickas med som actual outcome (win factor)
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
// I need five evened out values between 0 and 1.
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

// ACTUAL OUTCOME kanske också can be adjusted based on the number of participants.
// Kan den vara mer än 1 och mindre än 0?
// Alltså att vinna stort ger större rating
// Annars så kan alla deltagare lägga poäng i en pott (som ev baseras på k-factorn)
// och sedan fördelas den potten ut till deltagarna baserat på deras relativa position.


namespace SkillChallenge.Services
{
    public class EloRatingService
    {
        private readonly IRatingEntityRepository _ratingEntityRepository;
        private readonly IArchivedChallengeRepository _archivedChallengeRepository;

        int c_value = 400;

        public EloRatingService(IRatingEntityRepository ratingEntityRepository, IArchivedChallengeRepository archivedChallengeRepository)
        {
            _ratingEntityRepository = ratingEntityRepository;
            _archivedChallengeRepository = archivedChallengeRepository;
        }

        // Ser till att alla users i challengen har en rating i den
        // subCategory som challengen har. Om usern inte har någon rating i den
        // subCategory så får den en ny rating med värdet 1000.
        public async Task EnsureRatingsExistForParticipantsAsync(Challenge challenge, ICollection<User> users, int categoryId, int subCategoryId, CancellationToken ct)
        {
            var userList = users.ToList();

            // Build a set of UserNames who have uploaded results
            var uploadedUserNames = new HashSet<string>(challenge.UploadedResults.Select(ur => ur.User.UserName));

            // Remove users who have not uploaded results
            userList.RemoveAll(u => !uploadedUserNames.Contains(u.UserName));

            foreach (var user in userList)
            {
                var categoryRatingEntity = user.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);

                if (categoryRatingEntity == null)
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
                                Rating = 1000,
                                UserId = user.Id
                            }
                        }
                    };
                    await _ratingEntityRepository.AddAsync(newCategoryRating, ct);
                    user.CategoryRatingEntities.Add(newCategoryRating);
                }
                else
                {
                    var hasSubCategory = categoryRatingEntity.SubCategoryRatingEntities.Any(s => s.SubCategoryId == subCategoryId);

                    if (!hasSubCategory)
                    {
                        var newSubCategoryRating = new SubCategoryRatingEntity
                        {
                            SubCategoryId = subCategoryId,
                            Rating = 1000,
                            UserId = user.Id
                        };
                        categoryRatingEntity.SubCategoryRatingEntities.Add(newSubCategoryRating);
                        await _ratingEntityRepository.UpdateAsync(categoryRatingEntity, ct);
                    }
                }
            }
        }

        public async Task UpdateEloRatingsAsync(IEnumerable<User> usersEnumerable, int categoryId, int subCategoryId, Challenge challenge, CancellationToken ct)
        {
            var users = usersEnumerable.ToList();

            // Build a set of UserNames who have uploaded results
            var uploadedUserNames = new HashSet<string>(challenge.UploadedResults.Select(ur => ur.User.UserName));

            // Remove users who have not uploaded results
            users.RemoveAll(u => !uploadedUserNames.Contains(u.UserName));

            // This equation sets the k-factor adjusted for number of users.
            // k-factor rises faster with low number of users
            // 2 users - k-factor is 32
            // 5 users - k-factor is 40
            // 10 users - k-factor is 49
            // 100 users - k-factor is 116
            // 1000 users - k-factor is 329
            double k_factor = 18.1 + 260.0 * Math.Sqrt(users.Count() / 700.0);

            // Sort uploaded results by votes ascending
            var uploadedResults = challenge.UploadedResults.OrderBy(ur => ur.Votes.Count).ToList();

            if (users.Count() < 2 || uploadedResults.Count < 2)
            {
                return;
            }

            int actualPartcipantCount = users.Count();

            // participantFactor is a multiplier used to calculate the win_factor by a user
            // participantFactor will later be multiplied with the index position of the user to get the users win_factor
            float participantFactor = 1f / (actualPartcipantCount - 1);

            // Skapar nya object som består av
            // UploadedResult,
            // indexet det har i uploadedResults-listan,
            // och winFactor som är indexet multiplicerat med participantFactor
            // Exempel: 
            // participantFactor = 1 / 2 = 0.5
            // Astor 7 röster (index 2) (winfactor = 2 * 0.5 = 1)
            // Maggy 3 röster (index 1) (winfactor = 1 * 0.5 = 0.5)
            // Tammy 3 röster (index 0) (winfactor = 0 * 0.5 = 0)
            var winFactorByResult = uploadedResults
                .Select((ur, idx) =>
                new { ur, idx, winFactor = idx * participantFactor }).ToList();

            // Skapar en dictionary där antal röster är key
            // och value är genomsnittet på winFactors
            // i de winFactorByResult objekt som har lika många röster
            // Exempel:
            // Astors bidrag är det enda med 7 röster men gruperas ändå i en grupp med endast en medlem.
            // Genomsnittet på winfactorn hos alla de som har 4 tas fram. Den är 1.
            // Så key blir 4 och value blir 1.
            // Maggys och Tammys bidrag har båda 3 röster var. De hamnar i samma grupp.
            // Key för den gruppen är 3.
            // Ta nu fram snittet på winFactors från alla medlemmar i gruppen.
            // 0.5 + 0 / antal medlemmar (2) = 0.25
            // Key är 3 och value är 0.25
            var winFactorByVotes = winFactorByResult
                .GroupBy(x => x.ur.Votes.Count)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(x => x.winFactor));

            // Creates a dictionary of placements where
            // userId is key and placement is value
            var sortedResults = challenge.UploadedResults.OrderByDescending(ur => ur.Votes.Count).ToList();
            var userPlacements = new Dictionary<string, int>();
            int currentPlacement = 1;
            int previousVotes = -1;
            for (int i = 0, place = 1; i < sortedResults.Count; i++, place++)
            {
                var ur = sortedResults[i];
                if (ur.Votes.Count != previousVotes)
                {
                    currentPlacement = place;
                    previousVotes = ur.Votes.Count;
                }
                userPlacements[ur.UserId] = currentPlacement;
            }

            // Stores the new ratings before applying them in the foreach loop
            var newRatings = new Dictionary<SubCategoryRatingEntity, int>();

            // Get rating sum of all participants
            int sumOfAllParticipantsRating = 0;
            foreach (var user in users)
            {
                var userCategoryRatingEntity = user.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);
                var userSubCategoryRatingEntity = userCategoryRatingEntity.SubCategoryRatingEntities.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
                if (userSubCategoryRatingEntity == null) continue;
                int userRating = userSubCategoryRatingEntity.Rating;
                sumOfAllParticipantsRating += userRating;
            }

            var archivedUsers = new List<ArchivedChallengeUser>();

            // Här inne tas varje users nya rating fram
            foreach (var user in users)
            {
                // Find the uploaded result for this user
                var userResult = uploadedResults.FirstOrDefault(ur => ur.UserId == user.Id);
                if (userResult == null) continue;

                // Här används den dictionaryn vi pratade om tidigare.
                // Där key är antal röster
                // Och value är snittet på winFactorn på de bidrag som har det antalet röster
                float userWinFactor = winFactorByVotes[userResult.Votes.Count];

                // Gets the index of that users vote count
                //int position = uploadedResults.IndexOf(userResult); // Commented out 25-09-21

                // If userWinFactor is NaN it should mean that participantFactor had a divide by zero
                // which should mean that all uploaded results had the same number of votes.
                // Meaning its a complete draw.
                if (float.IsNaN(userWinFactor))
                {
                    userWinFactor = 0.5f;
                }

                // Ta fram rating för aktuell user, vi kallar den old rating för den ska ju uppdateras
                var categoryRatingEntity = user.CategoryRatingEntities.FirstOrDefault(c => c.CategoryId == categoryId);
                var subCategoryRatingEntity = categoryRatingEntity.SubCategoryRatingEntities.FirstOrDefault(s => s.SubCategoryId == subCategoryId);
                if (subCategoryRatingEntity == null) continue;
                int old_user_rating = subCategoryRatingEntity.Rating;

                // Ta fram genomsnittlig rating för alla andra participants förutom aktuell user
                sumOfAllParticipantsRating -= old_user_rating; // Remove self rating
                int opponentRatingAverage = (int)Math.Round((double)sumOfAllParticipantsRating / (actualPartcipantCount - 1));
                sumOfAllParticipantsRating += old_user_rating; // Put it back in for the next iteration

                float user_prevalue = get_prevalue(old_user_rating, c_value);
                float opponent_prevalue = get_prevalue(opponentRatingAverage, c_value);
                float user_win_chance = get_win_chance(user_prevalue, opponent_prevalue);
                int new_rating = get_new_rating(old_user_rating, k_factor, userWinFactor, user_win_chance);
                newRatings[subCategoryRatingEntity] = new_rating;

                int ratingChange = new_rating - old_user_rating;
                archivedUsers.Add(new ArchivedChallengeUser
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? "",
                    RatingChange = ratingChange,
                    Placement = userPlacements[user.Id]
                });
            }

            var archivedChallenge = new ArchivedChallenge
            {
                ChallengeId = challenge.ChallengeId,
                ChallengeName = challenge.ChallengeName,
                Description = challenge.Description,
                SubCategoryName = challenge.SubCategory?.SubCategoryName ?? "",
                EndDate = challenge.EndDate,
                Users = archivedUsers
            };

            await _archivedChallengeRepository.CreateArchivedChallengeAsync(archivedChallenge, ct);

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
        public static int get_new_rating(int old_rating, double k_factor, float winFactor, float user_win_chance)
        {
            double rating_to_be_transfered = k_factor * (winFactor - user_win_chance);
            double new_rating = old_rating + rating_to_be_transfered;
            int new_rating_as_int = (int)Math.Round(new_rating);
            return new_rating_as_int;
        }
    }
}
