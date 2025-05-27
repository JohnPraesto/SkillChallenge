using System.Net;
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
    public async Task TestRegisterNewUserSuccessfully()
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

    [Fact]
    public async Task TestRegisterUsernameAlreadyExists()
    {
        // Arrange
        var (client, _) = await SetupTestClient();

        var newUser = new RegisterUserDTO
        {
            Username = "testuser1",
            Email = "testuser1@example.com",
            Password = "NewPass123!",
        };

        // Act
        var response = await client.PostAsJsonAsync("/account/register", newUser);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("already taken", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("testuser1", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TestRegisterInvalidUserModel()
    {
        // Arrange
        var (client, _) = await SetupTestClient();

        var newUser = new RegisterUserDTO
        {
            Username = "", // Invalid
            Email = "bademail", // Invalid
            Password = "123", // Too short / invalid
        };

        // Act
        var response = await client.PostAsJsonAsync("/account/register", newUser);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TestLoginSuccessful()
    {
        // Arrange
        var (client, _) = await SetupTestClient();

        var loginDto = new LoginDTO { UserName = "testuser1", Password = "Password123!" };

        // Act
        var response = await client.PostAsJsonAsync("/account/login", loginDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<NewUserDTO>();
        Assert.NotNull(user);
        Assert.Equal("testuser1", user!.UserName);
        Assert.False(string.IsNullOrWhiteSpace(user.Token));
    }

    [Fact]
    public async Task TestLoginInvalidUsername()
    {
        // Arrange
        var (client, _) = await SetupTestClient();

        var loginDto = new LoginDTO { UserName = "nonexistentuser", Password = "DoesNotMatter123" };

        // Act
        var response = await client.PostAsJsonAsync("/account/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid username", body);
    }

    [Fact]
    public async Task TestLoginWrongPassword()
    {
        // Arrange
        var (client, _) = await SetupTestClient();

        var loginDto = new LoginDTO { UserName = "testuser1", Password = "WrongPassword123" };

        // Act
        var response = await client.PostAsJsonAsync("/account/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Username not found or password incorrect", body);
    }
}
