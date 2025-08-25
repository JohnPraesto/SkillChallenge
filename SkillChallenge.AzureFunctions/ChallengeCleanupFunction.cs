using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SkillChallenge.Data;

namespace SkillChallenge.AzureFunctions
{
    public class ChallengeCleanupFunction
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;

        public ChallengeCleanupFunction(ILoggerFactory loggerFactory, AppDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger<ChallengeCleanupFunction>();
            _dbContext = dbContext;
        }

        [Function("DeleteChallengesWithPastIsTakenDownDate")]
        public void Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            var now = DateTime.UtcNow;
            var oldChallenges = _dbContext.Challenges.Where(c => c.IsTakenDown < now).ToList();

            if (oldChallenges.Any())
            {
                _dbContext.Challenges.RemoveRange(oldChallenges);
                _dbContext.SaveChanges();
                _logger.LogInformation($"Deleted {oldChallenges.Count} old challenges.");
            }

            _logger.LogInformation($"C# Timer trigger function executed at: {now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
