using Microsoft.EntityFrameworkCore;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CategoryExistsAsync(int id, CancellationToken ct = default) =>
            await _context.Categories.AnyAsync(c => c.CategoryId == id, ct);

        public async Task<Category> CreateCategoryAsync(
            Category category,
            CancellationToken ct = default
        )
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync(ct);
            return category;
        }

        public async Task<Category?> DeleteCategoryAsync(int id, CancellationToken ct = default)
        {
            var category = await _context.Categories.FindAsync([id, ct]);
            if (category is null)
                return null;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(ct);
            return category;
        }

        public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken ct = default) =>
            await _context.Categories.Include(c => c.SubCategories).AsNoTracking().ToListAsync(ct);

        public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken ct = default) =>
            await _context
                .Categories.Include(c => c.SubCategories)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id, ct);

        public async Task<Category?> UpdateCategoryAsync(
            int id,
            Category updatedCategory,
            CancellationToken ct = default
        )
        {
            var existing = await _context.Categories.FirstOrDefaultAsync(
                c => c.CategoryId == id,
                ct
            );
            if (existing is null)
                return null;

            existing.CategoryName = updatedCategory.CategoryName;
            await _context.SaveChangesAsync(ct);
            return existing;
        }
    }
}
