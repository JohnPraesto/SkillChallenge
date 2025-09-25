using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IArchivedChallengeRepository
    {
        Task<List<ArchivedChallenge>> GetAllArchivedChallengesAsync(CancellationToken ct = default);
        Task<List<ArchivedChallenge>> GetArchivedChallengesByUserIdAsync(string userId, CancellationToken ct = default);
        Task<ArchivedChallenge> CreateArchivedChallengeAsync(ArchivedChallenge archivedChallenge, CancellationToken ct = default);
        Task<ArchivedChallengeUser> CreateArchivedChallengeUserAsync(ArchivedChallengeUser archivedChallengeUser, CancellationToken ct = default);
        Task<ArchivedChallenge?> DeleteArchivedChallengeAsync(int id, CancellationToken ct = default);
    }
}
