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

            // Roller
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

            var adminUser = new User
            {
                Id = "admin-123",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@skillchallenge.com",
                NormalizedEmail = "ADMIN@SKILLCHALLENGE.COM",
                EmailConfirmed = true,
                SecurityStamp = "STATIC-ADMIN-SECURITY-STAMP",
                ConcurrencyStamp = "STATIC-ADMIN-CONCURRENCY-STAMP",
                ProfilePicture = "",
                PasswordHash =
                    "AQAAAAIAAYagAAAAEIzZ1ipYa+9PoN6PNCJektB+44UdZJWEv/RnJtum84hmALg1Z4Gl5h9C0nDM2CIXOw==",
            };

            var testUser = new User
            {
                Id = "user-456",
                UserName = "testuser",
                NormalizedUserName = "TESTUSER",
                Email = "test@skillchallenge.com",
                NormalizedEmail = "TEST@SKILLCHALLENGE.COM",
                EmailConfirmed = true,
                SecurityStamp = "STATIC-USER-SECURITY-STAMP",
                ConcurrencyStamp = "STATIC-USER-CONCURRENCY-STAMP",
                ProfilePicture = "",
                PasswordHash =
                    "AQAAAAIAAYagAAAAECPJaSFhPkxbqX8QWGU013AN7zVInxVWKQ92xSKUPYH5LK7TTPhZQLFCAmjFOEKumg==",
            };

            builder.Entity<User>().HasData(adminUser, testUser);

            builder
                .Entity<IdentityUserRole<string>>()
                .HasData(
                    new IdentityUserRole<string> { RoleId = "1", UserId = "admin-123" },
                    new IdentityUserRole<string> { RoleId = "2", UserId = "user-456" }
                );

            // Challenge -> User
            builder
                .Entity<Challenge>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Challenge -> UnderCategory
            builder
                .Entity<Challenge>()
                .HasOne(c => c.UnderCategory)
                .WithMany()
                .HasForeignKey(c => c.UnderCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // UnderCategory -> Category
            builder
                .Entity<UnderCategory>()
                .HasOne(uc => uc.Category)
                .WithMany(c => c.UnderCategories)
                .HasForeignKey(uc => uc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many relation mellan Challenge och User
            builder
                .Entity<Challenge>()
                .HasMany(c => c.Users)
                .WithMany()
                .UsingEntity(j => j.ToTable("ChallengeUsers"));
        }
    }
}
