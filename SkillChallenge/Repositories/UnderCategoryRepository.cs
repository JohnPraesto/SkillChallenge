using Microsoft.EntityFrameworkCore;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Repositories
{
    public class UnderCategoryRepository : IUnderCategoryRepository
    {
        private readonly AppDbContext _context;

        public UnderCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UnderCategory> CreateUnderCategoryAsync(
            UnderCategory underCategory,
            CancellationToken ct = default
        )
        {
            _context.UnderCategories.Add(underCategory);
            await _context.SaveChangesAsync(ct);
            return underCategory;
        }

        public async Task<UnderCategory?> DeleteUnderCategoryAsync(
            int id,
            CancellationToken ct = default
        )
        {
            var underCategory = await _context.UnderCategories.FindAsync([id, ct]);
            if (underCategory is null)
                return null;

            _context.UnderCategories.Remove(underCategory);
            await _context.SaveChangesAsync(ct);
            return underCategory;
        }

        public async Task<List<UnderCategory>> GetAllUnderCategoriesAsync(
            CancellationToken ct = default
        ) =>
            await _context
                .UnderCategories.Include(uc => uc.Category)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<UnderCategory?> GetUnderCategoryByIdAsync(
            int id,
            CancellationToken ct = default
        ) =>
            await _context
                .UnderCategories.Include(uc => uc.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(uc => uc.UnderCategoryId == id, ct);

        public async Task<List<UnderCategory>> GetUnderCategoriesByCategoryIdAsync(
            int categoryId,
            CancellationToken ct = default
        ) =>
            await _context
                .UnderCategories.Include(uc => uc.Category)
                .Where(uc => uc.CategoryId == categoryId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<bool> UnderCategoryExistsAsync(int id, CancellationToken ct = default) =>
            await _context.UnderCategories.AnyAsync(uc => uc.UnderCategoryId == id, ct);

        public async Task<UnderCategory?> UpdateUnderCategoryAsync(
            int id,
            UnderCategory updatedUnderCategory,
            CancellationToken ct = default
        )
        {
            var existing = await _context.UnderCategories.FirstOrDefaultAsync(
                uc => uc.UnderCategoryId == id,
                ct
            );
            if (existing is null)
                return null;

            existing.UnderCategoryName = updatedUnderCategory.UnderCategoryName;
            existing.CategoryId = updatedUnderCategory.CategoryId;
            await _context.SaveChangesAsync(ct);
            return existing;
        }
    }
}
