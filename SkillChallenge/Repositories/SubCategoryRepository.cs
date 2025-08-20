using Microsoft.EntityFrameworkCore;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Repositories
{
    public class SubCategoryRepository : ISubCategoryRepository
    {
        private readonly AppDbContext _context;

        public SubCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SubCategory> CreateSubCategoryAsync(
            SubCategory subCategory,
            CancellationToken ct = default
        )
        {
            _context.SubCategories.Add(subCategory);
            await _context.SaveChangesAsync(ct);
            return subCategory;
        }

        public async Task<SubCategory?> DeleteSubCategoryAsync(
            int id,
            CancellationToken ct = default
        )
        {
            var subCategory = await _context.SubCategories.FindAsync([id, ct]);
            if (subCategory is null)
                return null;

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync(ct);
            return subCategory;
        }

        public async Task<List<SubCategory>> GetAllSubCategoriesAsync(
            CancellationToken ct = default
        ) => await _context.SubCategories.Include(uc => uc.Category).AsNoTracking().ToListAsync(ct);

        public async Task<SubCategory?> GetSubCategoryByIdAsync(
            int id,
            CancellationToken ct = default
        ) =>
            await _context
                .SubCategories.Include(uc => uc.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(uc => uc.SubCategoryId == id, ct);

        public async Task<List<SubCategory>> GetSubCategoriesByCategoryIdAsync(
            int categoryId,
            CancellationToken ct = default
        ) =>
            await _context
                .SubCategories.Include(uc => uc.Category)
                .Where(uc => uc.CategoryId == categoryId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<bool> SubCategoryExistsAsync(int id, CancellationToken ct = default) =>
            await _context.SubCategories.AnyAsync(uc => uc.SubCategoryId == id, ct);

        public async Task<SubCategory?> UpdateSubCategoryAsync(int id, SubCategory updatedSubCategory, CancellationToken ct = default)
        {
            var existing = await _context.SubCategories.FirstOrDefaultAsync(uc => uc.SubCategoryId == id, ct);
            if (existing is null)
                return null;

            existing.SubCategoryName = updatedSubCategory.SubCategoryName;
            existing.ImagePath = updatedSubCategory.ImagePath;
            existing.CategoryId = updatedSubCategory.CategoryId;

            await _context.SaveChangesAsync(ct);
            return existing;
        }
    }
}
