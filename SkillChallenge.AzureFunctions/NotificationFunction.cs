using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;

namespace SkillChallenge.AzureFunctions
{
    public class NotificationFunction
    {
        private readonly AppDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationFunction> _logger;

        public NotificationFunction(AppDbContext dbContext, IEmailService emailService, ILogger<NotificationFunction> logger)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _logger = logger;
        }

        [Function("NotifyTwoDaysBeforeEndDate")]
        public async Task NotifyTwoDaysBeforeEndDate([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            var today = DateTime.UtcNow.Date;
            var challenges = await _dbContext.Challenges
                .Where(c => c.EndDate.AddDays(-2) == today)
                .Include(c => c.Participants)
                .ToListAsync();

            foreach (var challenge in challenges)
            {
                var usersToNotify = challenge.Participants
                    .Where(u => u.NotifyTwoDaysBeforeEndDate)
                    .ToList();

                foreach (var user in usersToNotify)
                {
                    try
                    {
                        string challengeUrl = $"https://skillchallenge.net/challenges/{challenge.ChallengeId}";

                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Challenge voting begins soon",
                            $"Voting for your challenge <a href='{challengeUrl}'>{challenge.ChallengeName}</a> will begin in two days!" +
                            $"\nMake sure you have uploaded your results! =)"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send notification email to user {user.Id} ({user.UserName}) ({user.Email}) for challenge {challenge.ChallengeId} {challenge.ChallengeName}.");
                    }
                }
            }
        }

        [Function("NotifyVotingBegins")]
        public async Task NotifyVotingBegins([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            var today = DateTime.UtcNow.Date;
            var challenges = await _dbContext.Challenges
                .Where(c => c.EndDate == today)
                .Include(c => c.Participants)
                .ToListAsync();

            foreach (var challenge in challenges)
            {
                var usersToNotify = challenge.Participants
                    .Where(u => u.NotifyOnEndDate)
                    .ToList();

                foreach (var user in usersToNotify)
                {
                    try
                    {
                        string challengeUrl = $"https://skillchallenge.net/challenges/{challenge.ChallengeId}";

                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Challenge voting begins now!",
                            $"Voting for your challenge <a href='{challengeUrl}'>{challenge.ChallengeName}</a> has begun!" +
                            $"\nGet in there and place your vote, champ! =)"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send notification email to user {user.Id} ({user.UserName}) ({user.Email}) for challenge {challenge.ChallengeId} {challenge.ChallengeName}.");
                    }
                }
            }
        }

        [Function("NotifyVotingEnds")]
        public async Task NotifyVotingEnds([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            var today = DateTime.UtcNow.Date;
            var challenges = await _dbContext.Challenges
                .Where(c => c.VotePeriodEnd.Date == today)
                .Include(c => c.Participants)
                .ToListAsync();

            foreach (var challenge in challenges)
            {
                var usersToNotify = challenge.Participants
                    .Where(u => u.NotifyOnVotingEnd)
                    .ToList();

                foreach (var user in usersToNotify)
                {
                    try
                    {
                        string challengeUrl = $"https://skillchallenge.net/challenges/{challenge.ChallengeId}";

                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Challenge finished! Who won?",
                            $"Voting is finished for challenge <a href='{challengeUrl}'>{challenge.ChallengeName}</a>!" +
                            $"\nCheck out the placements! =)"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send notification email to user {user.Id} ({user.UserName}) ({user.Email}) for challenge {challenge.ChallengeId} {challenge.ChallengeName}.");
                    }
                }
            }
        }
    }
}
