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
                .Include(c => c.UnderCategory)
                .Include(c => c.Creator)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<Challenge?> GetChallengeByIdAsync(
            int id,
            CancellationToken ct = default
        ) =>
            await _context
                .Challenges.Include(c => c.Users)
                .Include(c => c.UnderCategory)
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
            existing.TimePeriod = updatedChallenge.TimePeriod;
            existing.Description = updatedChallenge.Description;
            existing.IsPublic = updatedChallenge.IsPublic;
            existing.UnderCategoryId = updatedChallenge.UnderCategoryId;

            await _context.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<List<Challenge>> GetChallengesByCreatorAsync(
            string creatorId,
            CancellationToken ct = default
        ) =>
            await _context
                .Challenges.Include(c => c.Users)
                .Include(c => c.UnderCategory)
                .Include(c => c.Creator)
                .Where(c => c.CreatedBy == creatorId)
                .AsNoTracking()
                .ToListAsync(ct);
    }
}
