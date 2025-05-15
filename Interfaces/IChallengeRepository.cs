using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IChallengeRepository
    {
        Task<List<Challenge>> GetAllChallengesAsync();
        Task<Challenge?> GetChallengeByIdAsync(int id);
        Task<Challenge> CreateChallengeAsync(Challenge challenge);
        Task<Challenge?> UpdateChallengeAsync(int id, Challenge updatedChallenge);
        Task<Challenge?> DeleteChallengeAsync(int id);
        Task<bool> ChallengeExistsAsync(int id);
    }
}
