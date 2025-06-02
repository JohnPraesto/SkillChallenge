using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IUnderCategoryRepository
    {
        Task<List<UnderCategory>> GetAllUnderCategoriesAsync(CancellationToken ct = default);
        Task<List<UnderCategory>> GetUnderCategoriesByCategoryIdAsync(
            int categoryId,
            CancellationToken ct = default
        );
        Task<UnderCategory?> GetUnderCategoryByIdAsync(int id, CancellationToken ct = default);
        Task<UnderCategory> CreateUnderCategoryAsync(
            UnderCategory underCategory,
            CancellationToken ct = default
        );
        Task<UnderCategory?> UpdateUnderCategoryAsync(
            int id,
            UnderCategory updatedUnderCategory,
            CancellationToken ct = default
        );
        Task<UnderCategory?> DeleteUnderCategoryAsync(int id, CancellationToken ct = default);
        Task<bool> UnderCategoryExistsAsync(int id, CancellationToken ct = default);
    }
}
