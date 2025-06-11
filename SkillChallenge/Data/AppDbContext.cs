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
        public DbSet<SubCategory> SubCategories { get; set; }

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
            if (
                !Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Contains("Test")
                ?? true
            )
            {
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
            }

            builder
                .Entity<Category>()
                .HasData(
                    new Category
                    {
                        CategoryId = 1,
                        CategoryName = "Music",
                        ImagePath = "images/categories/music.png",
                    },
                    new Category
                    {
                        CategoryId = 2,
                        CategoryName = "Sport",
                        ImagePath = "images/categories/sport.png",
                    },
                    new Category
                    {
                        CategoryId = 3,
                        CategoryName = "Food",
                        ImagePath = "images/categories/food.png",
                    },
                    new Category
                    {
                        CategoryId = 4,
                        CategoryName = "Games",
                        ImagePath = "images/categories/games.png",
                    },
                    new Category
                    {
                        CategoryId = 5,
                        CategoryName = "Other",
                        ImagePath = "images/categories/other.png",
                    }
                );

            builder
                .Entity<SubCategory>()
                .HasData(
                    new SubCategory
                    {
                        SubCategoryId = 1,
                        CategoryId = 1,
                        SubCategoryName = "Guitar",
                        ImagePath = "images/categories/music.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 2,
                        CategoryId = 1,
                        SubCategoryName = "Vocals",
                        ImagePath = "images/categories/music.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 3,
                        CategoryId = 2,
                        SubCategoryName = "Wrestling",
                        ImagePath = "images/categories/sport.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 4,
                        CategoryId = 2,
                        SubCategoryName = "Football",
                        ImagePath = "images/categories/sport.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 5,
                        CategoryId = 3,
                        SubCategoryName = "Recepies",
                        ImagePath = "images/categories/food.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 6,
                        CategoryId = 3,
                        SubCategoryName = "Baking",
                        ImagePath = "images/categories/food.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 7,
                        CategoryId = 4,
                        SubCategoryName = "Counter-Strike",
                        ImagePath = "images/categories/gaming.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 8,
                        CategoryId = 4,
                        SubCategoryName = "Chess",
                        ImagePath = "images/categories/gaming.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 9,
                        CategoryId = 5,
                        SubCategoryName = "Home design",
                        ImagePath = "images/categories/other.png",
                    },
                    new SubCategory
                    {
                        SubCategoryId = 10,
                        CategoryId = 5,
                        SubCategoryName = "Clothes",
                        ImagePath = "images/categories/other.png",
                    });


            builder
                    .Entity<IdentityUserRole<string>>()
                    .HasData(
                        new IdentityUserRole<string> { RoleId = "1", UserId = "admin-123" },
                        new IdentityUserRole<string> { RoleId = "2", UserId = "user-456" }
                    );

            // Challenge -> User (Creator)
            builder
                .Entity<Challenge>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Challenge -> SubCategory
            builder
                .Entity<Challenge>()
                .HasOne(c => c.SubCategory)
                .WithMany()
                .HasForeignKey(c => c.SubCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // SubCategory -> Category
            builder
                .Entity<SubCategory>()
                .HasOne(uc => uc.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(uc => uc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many relation mellan Challenge och User
            builder
                .Entity<Challenge>()
                .HasMany(c => c.Users)
                .WithMany(u => u.Challenges)
                .UsingEntity(j => j.ToTable("ChallengeUsers"));
        }
    }
}
