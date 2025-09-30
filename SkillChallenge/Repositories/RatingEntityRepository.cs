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
        public async Task<bool> SaveNewSubCategoryRatingEntityAsync(SubCategoryRatingEntity newSubCategoryRatingEntity, CancellationToken ct = default)
        {
            await _context.SubCategoryRatingEntities.AddAsync(newSubCategoryRatingEntity, ct);
            var result = await _context.SaveChangesAsync(ct);
            return result > 0;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
        {
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

        public async Task UpdateAsync(CategoryRatingEntity entity, CancellationToken ct)
        {
            _context.CategoryRatingEntities.Update(entity);
            await _context.SaveChangesAsync(ct);
        }
        public async Task DeleteRatingsForUserAsync(string userId, CancellationToken ct = default)
        {
            var categoryRatings = await _context.CategoryRatingEntities
                .Where(cre => cre.UserId == userId)
                .Include(cre => cre.SubCategoryRatingEntities)
                .ToListAsync(ct);

            foreach (var cre in categoryRatings)
            {
                _context.SubCategoryRatingEntities.RemoveRange(cre.SubCategoryRatingEntities);
                _context.CategoryRatingEntities.Remove(cre);
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
