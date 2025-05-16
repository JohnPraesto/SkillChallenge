using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillChallenge.Data;
using SkillChallenge.Models;
using System.Net.Http.Json;

public class UserIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, IServiceProvider serviceProvider)> SetupTestClientWithSeedData(Action<AppDbContext>? customSeed = null)
    {
        IServiceProvider? testServiceProvider = null;

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                             || d.ServiceType!.FullName!.Contains("DbContextOptions"))
                    .ToList();

                foreach (var d in descriptors)
                    services.Remove(d);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                var sp = services.BuildServiceProvider();
                testServiceProvider = sp;

                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // Use default seed if no custom seed provided
                (customSeed ?? DefaultSeed)(db);
                db.SaveChanges();
            });
        }).CreateClient();

        return (client, testServiceProvider!);
    }

    private void DefaultSeed(AppDbContext db)
    {
        db.Users.AddRange(
            new User { UserId = 1, UserName = "TestUser1", Password = "TestPassword1", ProfilePicture = "Pic1" },
            new User { UserId = 2, UserName = "TestUser2", Password = "TestPassword2", ProfilePicture = "Pic2" }
        );
    }

    [Fact]
    public async Task TestGetAllUsers()
    {
        // Arrange
        var (client, _) = await SetupTestClientWithSeedData();

        // Act
        var response = await client.GetAsync("/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.Equal(2, users!.Count);
        Assert.Contains(users, u => u.UserName == "TestUser1");
        Assert.Equal("TestUser2", users[1].UserName);
    }

    [Fact]
    public async Task TestCreateUser()
    {
        // Arrange
        var (client, services) = await SetupTestClientWithSeedData();

        var newUser = new User
        {
            UserId = 3,
            UserName = "TestUser3",
            Password = "TestPassword3",
            ProfilePicture = "Pic3"
        };

        // Act
        var response = await client.PostAsJsonAsync("/users", newUser);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<User>();
        Assert.Equal("TestUser3", result!.UserName);

        // Verify it was stored in the DB
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = await db.Users.FindAsync(3);
        Assert.NotNull(userInDb);
    }
}
