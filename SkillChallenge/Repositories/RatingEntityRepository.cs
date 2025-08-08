using Microsoft.EntityFrameworkCore;
using SkillChallenge.Data;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Repositories
{
    public class RatingEntityRepository : IRatingEntityRepository
    {
        private readonly AppDbContext _context;

        public RatingEntityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(CategoryRatingEntity newRatingEntity, CancellationToken ct = default)
        {
            await _context.CategoryRatingEntities.AddAsync(newRatingEntity, ct);
            var result = await _context.SaveChangesAsync(ct);
            return result > 0;
        }
        public async Task<bool> SetNewSubCategoryRatingAsync(SubCategoryRatingEntity subCategoryRatingEntity, int newRating, CancellationToken ct = default)
        {
            // Attach if not tracked (optional, for robustness)
            if (_context.Entry(subCategoryRatingEntity).State == EntityState.Detached)
            {
                _context.SubCategoryRatingEntities.Attach(subCategoryRatingEntity);
            }
            subCategoryRatingEntity.Rating = newRating;
            var result = await _context.SaveChangesAsync(ct);
            return result > 0;
        }
    }
}
