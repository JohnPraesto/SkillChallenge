using Microsoft.EntityFrameworkCore;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Repositories
{
    public class ArchivedChallengeRepository : IArchivedChallengeRepository
    {
        private readonly AppDbContext _context;

        public ArchivedChallengeRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ArchivedChallenge>> GetAllArchivedChallengesAsync(CancellationToken ct = default)
        {
            return await _context.ArchivedChallenges
                .Include(ac => ac.Users)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<List<ArchivedChallenge>> GetArchivedChallengesByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _context.ArchivedChallenges
                .Include(ac => ac.Users)
                .Where(ac => ac.Users.Any(u => u.UserId == userId))
                .AsNoTracking()
                .ToListAsync(ct);
        }
        public async Task<ArchivedChallenge> CreateArchivedChallengeAsync(ArchivedChallenge archivedChallenge, CancellationToken ct = default)
        {
            _context.ArchivedChallenges.Add(archivedChallenge);
            await _context.SaveChangesAsync(ct);
            return archivedChallenge;
        }

        public async Task<ArchivedChallengeUser> CreateArchivedChallengeUserAsync(ArchivedChallengeUser archivedChallengeUser, CancellationToken ct = default)
        {
            _context.ArchivedChallengeUsers.Add(archivedChallengeUser);
            await _context.SaveChangesAsync(ct);
            return archivedChallengeUser;
        }

        public async Task<ArchivedChallenge?> DeleteArchivedChallengeAsync(int id, CancellationToken ct = default)
        {
            var archivedChallenge = await _context.ArchivedChallenges.FindAsync([id, ct]);
            if (archivedChallenge is null)
                return null;

            _context.ArchivedChallenges.Remove(archivedChallenge);
            await _context.SaveChangesAsync(ct);
            return archivedChallenge;
        }
    }
}
