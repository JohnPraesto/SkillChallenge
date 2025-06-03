using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IChallengeRepository
    {
        Task<List<Challenge>> GetAllChallengesAsync(CancellationToken ct = default);
        Task<Challenge?> GetChallengeByIdAsync(int id, CancellationToken ct = default);
        Task<Challenge> CreateChallengeAsync(Challenge challenge, CancellationToken ct = default);
        Task<Challenge?> UpdateChallengeAsync(
            int id,
            Challenge updatedChallenge,
            CancellationToken ct = default
        );
        Task<Challenge?> DeleteChallengeAsync(int id, CancellationToken ct = default);
        Task<bool> ChallengeExistsAsync(int id, CancellationToken ct = default);
        Task<List<Challenge>> GetChallengesByCreatorAsync(
            string creatorId,
            CancellationToken ct = default
        );
    }
}
