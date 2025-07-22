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
                .Challenges.Include(c => c.Users)
                .Include(c => c.SubCategory)
                .Include(c => c.Creator)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<Challenge?> GetChallengeByIdAsync(
            int id,
            CancellationToken ct = default
        ) =>
            await _context
                .Challenges.Include(c => c.Users)
                .Include(c => c.UploadedResults)
                .Include(c => c.SubCategory)
                .Include(c => c.Creator)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ChallengeId == id, ct);

        public async Task<Challenge?> UpdateChallengeAsync(
            int id,
            Challenge updatedChallenge,
            CancellationToken ct = default
        )
        {
            var existing = await _context.Challenges.FirstOrDefaultAsync(
                c => c.ChallengeId == id,
                ct
            );
            if (existing is null)
                return null;

            existing.ChallengeName = updatedChallenge.ChallengeName;
            existing.EndDate = updatedChallenge.EndDate;
            existing.Description = updatedChallenge.Description;
            existing.IsPublic = updatedChallenge.IsPublic;
            existing.SubCategoryId = updatedChallenge.SubCategoryId;

            await _context.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<List<Challenge>> GetChallengesByCreatorAsync(
            string creatorId,
            CancellationToken ct = default
        ) =>
            await _context
                .Challenges.Include(c => c.Users)
                .Include(c => c.SubCategory)
                .Include(c => c.Creator)
                .Where(c => c.CreatedBy == creatorId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<bool> AddUserToChallengeAsync(int challengeId, string userId, CancellationToken ct = default)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.ChallengeId == challengeId, ct);
            if (challenge == null) return false;

            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null) return false;

            if (!challenge.Users.Contains(user))
            {
                challenge.Users.Add(user);
                await _context.SaveChangesAsync(ct);
            }
            return true;
        }

        public async Task<bool> RemoveUserFromChallengeAsync(int challengeId, string userId, CancellationToken ct = default)
        {
            var challenge = await _context.Challenges
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.ChallengeId == challengeId, ct);
            if (challenge == null) return false;

            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null) return false;

            if (challenge.Users.Contains(user))
            {
                challenge.Users.Remove(user);
                await _context.SaveChangesAsync(ct);
            }
            return true;
        }

        public async Task<bool> AddUploadedResultToChallengeAsync(int challengeId, UploadedResult uploadedResult, CancellationToken ct = default)
        {
            var challenge = await _context.Challenges.FirstOrDefaultAsync(c => c.ChallengeId == challengeId, ct);
            if (challenge == null) return false;

            var exists = await _context.UploadedResults.AnyAsync(ur => ur.ChallengeId == challengeId && ur.UserId == uploadedResult.UserId, ct);
            if (exists) return false;

            challenge.UploadedResults.Add(uploadedResult);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteUploadedResultAsync(int challengeId, int uploadedResultId, string userId, CancellationToken ct = default)
        {
            var uploadedResult = await _context.UploadedResults
                .FirstOrDefaultAsync(ur => ur.UploadedResultId == uploadedResultId && ur.ChallengeId == challengeId && ur.UserId == userId, ct);

            if (uploadedResult == null)
                return false;

            _context.UploadedResults.Remove(uploadedResult);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
