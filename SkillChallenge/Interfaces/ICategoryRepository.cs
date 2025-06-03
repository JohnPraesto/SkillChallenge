using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllCategoriesAsync(CancellationToken ct = default);
        Task<Category?> GetCategoryByIdAsync(int id, CancellationToken ct = default);
        Task<Category> CreateCategoryAsync(Category category, CancellationToken ct = default);
        Task<Category?> UpdateCategoryAsync(
            int id,
            Category updatedCategory,
            CancellationToken ct = default
        );
        Task<Category?> DeleteCategoryAsync(int id, CancellationToken ct = default);
        Task<bool> CategoryExistsAsync(int id, CancellationToken ct = default);
    }
}
