using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillChallenge.Data;
using SkillChallenge.Services;

namespace SkillChallenge.AzureFunctions
{
    public class ChallengeSubmitResultsFunction
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;
        private readonly EloRatingService _eloRatingService;

        public ChallengeSubmitResultsFunction(ILoggerFactory loggerFactory, AppDbContext dbContext, EloRatingService eloRatingService)
        {
            _logger = loggerFactory.CreateLogger<ChallengeSubmitResultsFunction>();
            _dbContext = dbContext;
            _eloRatingService = eloRatingService;
        }

        [Function("SubmitResultsForChallengesWithPastVotePeriodEndDate")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            var now = DateTime.UtcNow;
            var challengesToSubmit = await _dbContext.Challenges
                .Include(c => c.Participants)
                .Include(c => c.SubCategory)
                .Include(c => c.UploadedResults).ThenInclude(ur => ur.Votes)
                .Where(c => c.VotePeriodEnd < now && !c.ResultsSubmitted)
                .ToListAsync();

            foreach (var challenge in challengesToSubmit)
            {
                var categoryId = challenge.SubCategory.CategoryId;
                await _eloRatingService.EnsureRatingsExistForParticipantsAsync(challenge.Participants, categoryId, challenge.SubCategoryId.Value, CancellationToken.None);
                await _eloRatingService.UpdateEloRatingsAsync(challenge.Participants, categoryId, challenge.SubCategoryId.Value, challenge, CancellationToken.None);

                challenge.ResultsSubmitted = true;
                _logger.LogInformation($"Submitted results for challenge {challenge.ChallengeId}.");
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"SubmitResultsForPastVotePeriodEnd executed at: {now}");
        }
    }
}