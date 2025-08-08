using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IRatingEntityRepository
    {
        Task<bool> AddAsync(CategoryRatingEntity newRatingEntity, CancellationToken ct = default);
        Task<bool> SetNewSubCategoryRatingAsync(SubCategoryRatingEntity subCategoryRating, int newRating, CancellationToken ct = default);
    }
}
