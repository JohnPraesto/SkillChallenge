using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IRatingEntityRepository
    {
        Task<bool> AddAsync(CategoryRatingEntity newCategoryRatingEntity, CancellationToken ct = default);
        Task<bool> SaveNewSubCategoryRatingEntityAsync(SubCategoryRatingEntity subCategoryRatingEntity, CancellationToken ct = default);
        Task<bool> SaveChangesAsync(CancellationToken ct = default);
        Task<bool> SetNewSubCategoryRatingAsync(SubCategoryRatingEntity subCategoryRatingEntity, int newRating, CancellationToken ct = default);
        Task UpdateAsync(CategoryRatingEntity categoryRatingEntity, CancellationToken ct);
        Task DeleteRatingsForUserAsync(string userId, CancellationToken ct = default);
    }
}
