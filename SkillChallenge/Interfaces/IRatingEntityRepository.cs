using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IRatingEntityRepository
    {
        Task<bool> AddAsync(CategoryRatingEntity newRatingEntity, CancellationToken ct = default);
    }
}
