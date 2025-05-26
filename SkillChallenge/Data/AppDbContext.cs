using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillChallenge.Models;

namespace SkillChallenge.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions) { }

        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UnderCategory> UnderCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "User",
                    NormalizedName = "USER",
                },
            };
            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
