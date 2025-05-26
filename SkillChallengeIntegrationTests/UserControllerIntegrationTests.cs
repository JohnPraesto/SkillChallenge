using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillChallenge.Data;
using SkillChallenge.DTOs;
using SkillChallenge.Models;

public class UserControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserControllerIntegrationTests(WebApplicationFactory<Program> factory)
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

                    var dbName = Guid.NewGuid().ToString();
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(dbName)
                    );

                    var sp = services.BuildServiceProvider();
                    testServiceProvider = sp;

                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

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
    public async Task TestGetAllUsers()
    {
        // Arrange
        var (client, _) = await SetupTestClient();

        // Act
        var response = await client.GetAsync("/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.Equal(2, users!.Count);
        Assert.Contains(
            users,
            u => u.UserName == "testuser1" && u.Email == "testuser1@example.com"
        );
    }

    [Fact]
    public async Task TestGetUserByUsername()
    {
        // Arrange
        var (client, _) = await SetupTestClient();

        // Act
        var response = await client.GetAsync("/users/testuser2");

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(user);
        Assert.Equal("testuser2", user!.UserName);
    }

    [Fact]
    public async Task TestGetUserByUsernameNotFound()
    {
        // Arrange
        var (client, _) = await SetupTestClient();
        string testuser = "nonexistinguser";

        // Act
        var response = await client.GetAsync($"/users/{testuser}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains($"User with username '{testuser}' was not found in the database", content);
    }

    [Fact]
    public async Task TestDeleteUser()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.UserName == "testuser1");
        string userId = user.Id;

        // Act
        var response = await client.DeleteAsync($"/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var verifyScope = services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = await verifyDb.Users.FindAsync(userId);
        Assert.Null(userInDb);
    }

    [Fact]
    public async Task TestDeleteUserNotFound()
    {
        // Arrange
        var (client, _) = await SetupTestClient();
        string testId = "iddoesnotexist";

        // Act
        var response = await client.DeleteAsync($"/users/{testId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains($"User with id {testId} was not found in the database", content);
    }

    [Fact]
    public async Task TestUpdateUser()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.UserName == "testuser1");
        string userId = user.Id;

        string updateName = "UpdatedName";
        var userUpdate = new UpdateUserDTO
        {
            UserName = updateName,
            ProfilePicture = "UpdatedPicture",
        };

        // Act
        var response = await client.PutAsJsonAsync($"/users/{userId}", userUpdate);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(result);
        Assert.Equal(updateName, result.UserName);

        // Verify it was updated in the DB
        using var verifyScope = services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = await verifyDb.Users.FindAsync(userId);
        Assert.NotNull(userInDb);
        Assert.Equal("UpdatedPicture", userInDb.ProfilePicture);
        Assert.Equal(updateName, userInDb.UserName);
    }

    [Fact]
    public async Task TestUpdateUserNotFound()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        string userId = "doesnotexist";

        string updateName = "UpdatedName";
        var userUpdate = new UpdateUserDTO
        {
            UserName = updateName,
            ProfilePicture = "UpdatedPicture",
        };

        // Act
        var response = await client.PutAsJsonAsync($"/users/{userId}", userUpdate);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains($"User with id {userId} was not found in the database", content);
    }

    [Fact]
    public async Task TestChangePassword()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.UserName == "testuser1");
        string userId = user.Id;
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var changeDTO = new ChangePasswordDTO
        {
            CurrentPassword = "Password123!",
            NewPassword = "Password456?",
        };

        // Act
        var response = await client.PostAsJsonAsync($"/users/{userId}/change-password", changeDTO);

        // Assert
        response.EnsureSuccessStatusCode();

        // Reload user
        var updatedUser = await userManager.FindByIdAsync(userId);
        await db.Entry(updatedUser!).ReloadAsync();

        // Verify new password works
        var newPasswordValid = await userManager.CheckPasswordAsync(updatedUser!, "Password456?");
        Assert.True(newPasswordValid);

        // Also check that old password no longer works
        var oldPasswordValid = await userManager.CheckPasswordAsync(updatedUser!, "Password123!");
        Assert.False(oldPasswordValid);
    }
}
