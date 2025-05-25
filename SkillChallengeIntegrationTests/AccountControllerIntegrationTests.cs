using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillChallenge.Data;
using SkillChallenge.DTOs;
using SkillChallenge.Models;

public class AccountControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AccountControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, IServiceProvider serviceProvider)> SetupTestClient(
        Func<IServiceProvider, Task>? customSeed = null
    )
    {
        IServiceProvider? testServiceProvider = null;

        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services
                        .Where(d =>
                            d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                            || d.ServiceType!.FullName!.Contains("DbContextOptions")
                        )
                        .ToList();

                    foreach (var d in descriptors)
                        services.Remove(d);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb")
                    );

                    var sp = services.BuildServiceProvider();
                    testServiceProvider = sp;

                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // Seed users and roles using UserManager/RoleManager
                    var seedTask = (customSeed ?? DefaultSeedAsync)(scope.ServiceProvider);
                    seedTask.GetAwaiter().GetResult();
                });
            })
            .CreateClient();

        return (client, testServiceProvider!);
    }

    private async Task DefaultSeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        var testUser1 = new User { UserName = "testuser1", Email = "testuser1@example.com" };
        var testUser2 = new User { UserName = "testuser2", Email = "testuser2@example.com" };

        await userManager.CreateAsync(testUser1, "Password123!");
        await userManager.AddToRoleAsync(testUser1, "User");

        await userManager.CreateAsync(testUser2, "Password123!");
        await userManager.AddToRoleAsync(testUser2, "User");
    }

    [Fact]
    public async Task Register_ShouldCreateNewUserAndReturnToken()
    {
        // Arrange
        var (client, _) = await SetupTestClient();

        var newUser = new RegisterUserDTO
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "NewPass123!",
        };

        // Act
        var response = await client.PostAsJsonAsync("/account/register", newUser);

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<NewUserDTO>();
        Assert.NotNull(user);
        Assert.Equal("newuser", user!.UserName);
        Assert.Equal("newuser@example.com", user.Email);
        Assert.False(string.IsNullOrWhiteSpace(user.Token));
    }
}
