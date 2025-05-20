using Microsoft.EntityFrameworkCore;
using SkillChallenge.Models;

namespace SkillChallenge.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UnderCategory> UnderCategories { get; set; }
    }
}
