using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface ISubCategoryRepository
    {
        Task<List<SubCategory>> GetAllSubCategoriesAsync(CancellationToken ct = default);
        Task<List<SubCategory>> GetSubCategoriesByCategoryIdAsync(
            int categoryId,
            CancellationToken ct = default
        );
        Task<SubCategory?> GetSubCategoryByIdAsync(int id, CancellationToken ct = default);
        Task<SubCategory> CreateSubCategoryAsync(
            SubCategory subCategory,
            CancellationToken ct = default
        );
        Task<SubCategory?> UpdateSubCategoryAsync(
            int id,
            SubCategory updatedSubCategory,
            CancellationToken ct = default
        );
        Task<SubCategory?> DeleteSubCategoryAsync(int id, CancellationToken ct = default);
        Task<bool> SubCategoryExistsAsync(int id, CancellationToken ct = default);
    }
}
