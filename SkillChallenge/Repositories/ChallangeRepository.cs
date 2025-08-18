using Microsoft.EntityFrameworkCore;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Repositories
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly AppDbContext _context;

        public ChallengeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ChallengeExistsAsync(int id, CancellationToken ct = default) =>
            await _context.Challenges.AnyAsync(c => c.ChallengeId == id, ct);

        public async Task<Challenge> CreateChallengeAsync(
            Challenge challenge,
            CancellationToken ct = default
        )
        {
            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync(ct);
            return challenge;
        }

        public async Task<Challenge?> DeleteChallengeAsync(int id, CancellationToken ct = default)
        {
            var challenge = await _context.Challenges.FindAsync([id, ct]);
            if (challenge is null)
                return null;

            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync(ct);
            return challenge;
        }

        public async Task<List<Challenge>> GetAllChallengesAsync(CancellationToken ct = default) =>
            await _context
                .Challenges.Include(c => c.Participants)
                .Include(c => c.SubCategory)
                .Include(c => c.Creator)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<Challenge?> GetChallengeByIdAsync(
            int id,
            CancellationToken ct = default
        ) =>
            await _context.Challenges
                .Include(c => c.Participants)
                .Include(c => c.Participants).ThenInclude(u => u.CategoryRatingEntities)
                .Include(c => c.Participants).ThenInclude(u => u.CategoryRatingEntities).ThenInclude(cr => cr.SubCategoryRatingEntities)
                .Include(c => c.UploadedResults).ThenInclude(ur => ur.User)
                .Include(c => c.UploadedResults).ThenInclude(ur => ur.Votes)
                .Include(c => c.SubCategory)
                .Include(c => c.Creator)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ChallengeId == id, ct);

        public async Task<Challenge?> UpdateChallengeAsync(int id, Challenge updatedChallenge, CancellationToken ct = default)
        {
            var existing = await _context.Challenges.FirstOrDefaultAsync(
                c => c.ChallengeId == id,
                ct
            );
            if (existing is null)
                return null;

            existing.ChallengeName = updatedChallenge.ChallengeName;
            existing.EndDate = updatedChallenge.EndDate;
            existing.VotePeriodEnd = updatedChallenge.VotePeriodEnd;
            existing.IsTakenDown = updatedChallenge.IsTakenDown;
            existing.Description = updatedChallenge.Description;
            existing.NumberOfParticipants = updatedChallenge.NumberOfParticipants;
            existing.SubCategoryId = updatedChallenge.SubCategoryId;

            await _context.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<List<Challenge>> GetChallengesByCreatorAsync(
            string creatorId,
            CancellationToken ct = default
        ) =>
            await _context
                .Challenges.Include(c => c.Participants)
                .Include(c => c.SubCategory)
                .Include(c => c.Creator)
                .Where(c => c.CreatedBy == creatorId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<ChallengeJoinStatus> AddUserToChallengeAsync(int challengeId, string userId, CancellationToken ct = default)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ChallengeId == challengeId, ct);
            if (challenge == null) return ChallengeJoinStatus.ChallengeNotFound;

            if (challenge.Participants.Count >= challenge.NumberOfParticipants)
                return ChallengeJoinStatus.ChallengeFull;

            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null) return ChallengeJoinStatus.UserNotFound;

            if (challenge.Participants.Contains(user))
                return ChallengeJoinStatus.AlreadyJoined;

            challenge.Participants.Add(user);
            await _context.SaveChangesAsync(ct);
            return ChallengeJoinStatus.Success;
        }

        public async Task<bool> RemoveUserFromChallengeAsync(int challengeId, string userId, CancellationToken ct = default)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ChallengeId == challengeId, ct);
            if (challenge == null) return false;

            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null) return false;

            if (challenge.Participants.Contains(user))
            {
                challenge.Participants.Remove(user);
                await _context.SaveChangesAsync(ct);
            }
            return true;
        }

        public async Task<UploadResultStatus> AddUploadedResultToChallengeAsync(int challengeId, UploadedResult uploadedResult, CancellationToken ct = default)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Participants)
                .Include(c => c.UploadedResults)
                .FirstOrDefaultAsync(c => c.ChallengeId == challengeId, ct);

            if (challenge == null) return UploadResultStatus.ChallengeNotFound;

            if (challenge.EndDate < DateTime.UtcNow)
                return UploadResultStatus.ChallengeEnded;

            var hasJoined = challenge.Participants.Any(u => u.Id == uploadedResult.UserId);
            if (!hasJoined) return UploadResultStatus.NotJoined;

            var exists = await _context.UploadedResults.AnyAsync(ur => ur.ChallengeId == challengeId && ur.UserId == uploadedResult.UserId, ct);
            if (exists) return UploadResultStatus.AlreadyUploaded;

            _context.UploadedResults.Add(uploadedResult);
            await _context.SaveChangesAsync(ct);
            return UploadResultStatus.Success;
        }

        public async Task<bool> DeleteUploadedResultAsync(int challengeId, string userId, CancellationToken ct = default)
        {
            var uploadedResult = await _context.UploadedResults
                .FirstOrDefaultAsync(ur => ur.ChallengeId == challengeId && ur.UserId == userId, ct);

            if (uploadedResult == null)
                return false;

            _context.UploadedResults.Remove(uploadedResult);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AddOrMoveVoteAsync(int challengeId, int uploadedResultId, string userId, CancellationToken ct = default)
        {
            var existingVote = await _context.VoteEntities
                .FirstOrDefaultAsync(v => v.ChallengeId == challengeId && v.UserId == userId, ct);

            if (existingVote != null)
            {
                if (existingVote.UploadedResultId == uploadedResultId)
                {
                    // User is toggling their vote off (unvoting)
                    _context.VoteEntities.Remove(existingVote);
                    await _context.SaveChangesAsync(ct);
                    return true; // Vote removed
                }

                // Move vote to new uploaded result
                existingVote.UploadedResultId = uploadedResultId;
                await _context.SaveChangesAsync(ct);
                return true; // Vote moved
            }
            else
            {
                // Add new vote
                var newVote = new VoteEntity
                {
                    ChallengeId = challengeId,
                    UploadedResultId = uploadedResultId,
                    UserId = userId
                };
                _context.VoteEntities.Add(newVote);
                await _context.SaveChangesAsync(ct);
                return true;
            }
        }
    }
}
