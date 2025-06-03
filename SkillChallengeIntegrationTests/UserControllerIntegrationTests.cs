using System.Net;
using System.Net.Http.Headers;
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
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
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

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var testUser1 = new User { UserName = "testuser1", Email = "testuser1@example.com" };
        var testUser2 = new User { UserName = "testuser2", Email = "testuser2@example.com" };
        var testAdmin = new User { UserName = "testadmin", Email = "testadmin@example.com" };

        await userManager.CreateAsync(testUser1, "Password123!");
        await userManager.AddToRoleAsync(testUser1, "User");

        await userManager.CreateAsync(testUser2, "Password123!");
        await userManager.AddToRoleAsync(testUser2, "User");

        await userManager.CreateAsync(testAdmin, "Password123!");
        await userManager.AddToRoleAsync(testAdmin, "Admin");
    }

    // As part of the Arrange of the test,
    // this method provides the id of the user
    // to be operated on in the test
    private async Task<string> GetTestId(IServiceProvider services, string userName)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.UserName == userName);
        return user.Id;
    }

    // As part of the Arrange of the test,
    // this method provides the JWT token for the acting user
    // that logged in for the test
    private async Task<string> GetToken(HttpClient client, string user)
    {
        var loginDto = new LoginDTO { UserName = user, Password = "Password123!" };
        var loginResponse = await client.PostAsJsonAsync("/account/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<NewUserDTO>();
        return loginResult!.Token;
    }

    [Fact]
    public async Task TestCreateAdminWithAdmin()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        var token = await GetToken(client, "testadmin");

        var newAdmin = new RegisterUserDTO
        {
            Username = "newadmin",
            Email = "newadmin@example.com",
            Password = "NewAdminPass123!",
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/users/create-admin")
        {
            Content = JsonContent.Create(newAdmin),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdAdmin = await response.Content.ReadFromJsonAsync<DisplayUserDTO>();
        Assert.NotNull(createdAdmin);
        Assert.Equal("newadmin", createdAdmin.UserName);
        Assert.Equal("newadmin@example.com", createdAdmin.Email);

        // Verify in DB that user has Admin role
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var newAdminUser = await userManager.FindByNameAsync("newadmin");
        Assert.NotNull(newAdminUser);
        var roles = await userManager.GetRolesAsync(newAdminUser);
        Assert.Contains("Admin", roles);
    }

    [Fact]
    public async Task TestCreateAdminWithUser()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        var token = await GetToken(client, "testuser1");

        var newAdmin = new RegisterUserDTO
        {
            Username = "newadmin",
            Email = "newadmin@example.com",
            Password = "NewAdminPass123!",
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/users/create-admin")
        {
            Content = JsonContent.Create(newAdmin),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // Check that the user was not created in the DB
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var newAdminUser = await userManager.FindByNameAsync("newadmin");
        Assert.Null(newAdminUser);
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
        Assert.Equal(3, users!.Count);
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
    public async Task TestDeleteUserWithAdmin()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        string userToDeleteId = await GetTestId(services, "testuser1");
        var token = await GetToken(client, "testadmin");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{userToDeleteId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var verifyScope = services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = await verifyDb.Users.FindAsync(userToDeleteId);
        Assert.Null(userInDb);
    }

    [Fact]
    public async Task TestDeleteUserWithUser()
    {
        // Arrange
        var (client, services) = await SetupTestClient();

        string userToDeleteId = await GetTestId(services, "testuser1");
        var token = await GetToken(client, "testuser1");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{userToDeleteId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var verifyScope = services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = await verifyDb.Users.FindAsync(userToDeleteId);
        Assert.Null(userInDb);
    }

    [Fact]
    public async Task TestDeleteUserWithWrongUser()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        string userToDeleteId = await GetTestId(services, "testuser2");
        var token = await GetToken(client, "testuser1");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{userToDeleteId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var verifyScope = services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = await verifyDb.Users.FindAsync(userToDeleteId);
        Assert.NotNull(userInDb);
    }

    [Fact]
    public async Task TestDeleteUserNotFound()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        string testId = "iddoesnotexist";
        var token = await GetToken(client, "testadmin");

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{testId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains($"User with id {testId} was not found in the database", content);
    }

    [Fact]
    public async Task TestUpdateUserWithAdmin()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        string userId = await GetTestId(services, "testuser1");
        var token = await GetToken(client, "testadmin");

        string updateName = "UpdatedName";
        var userUpdate = new UpdateUserDTO
        {
            UserName = updateName,
            ProfilePicture = "UpdatedPicture",
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{userId}")
        {
            Content = JsonContent.Create(userUpdate),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

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
    public async Task TestUpdateUserWithUser()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        string userId = await GetTestId(services, "testuser1");
        var token = await GetToken(client, "testuser1");

        string updateName = "UpdatedName";
        var userUpdate = new UpdateUserDTO
        {
            UserName = updateName,
            ProfilePicture = "UpdatedPicture",
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{userId}")
        {
            Content = JsonContent.Create(userUpdate),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

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
    public async Task TestUpdateUserWithWrongUser()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        string userId = await GetTestId(services, "testuser2");
        var token = await GetToken(client, "testuser1");

        string updateName = "UpdatedName";
        var userUpdate = new UpdateUserDTO
        {
            UserName = updateName,
            ProfilePicture = "UpdatedPicture",
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{userId}")
        {
            Content = JsonContent.Create(userUpdate),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var verifyScope = services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = await verifyDb.Users.FindAsync(userId);
        Assert.NotNull(userInDb);
    }

    [Fact]
    public async Task TestUpdateUserNotFound()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        string userId = "doesnotexist";
        var token = await GetToken(client, "testadmin");

        string updateName = "UpdatedName";
        var userUpdate = new UpdateUserDTO
        {
            UserName = updateName,
            ProfilePicture = "UpdatedPicture",
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/users/{userId}")
        {
            Content = JsonContent.Create(userUpdate),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains($"User with id {userId} was not found in the database", content);
    }

    [Fact]
    public async Task TestChangePasswordWithAdmin()
    {
        // Arrange
        var (client, services) = await SetupTestClient();
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.UserName == "testuser1");
        string userId = user.Id;
        var token = await GetToken(client, "testadmin");
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var changeDTO = new ChangePasswordDTO
        {
            CurrentPassword = "Password123!",
            NewPassword = "Password456?",
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/{userId}/change-password")
        {
            Content = JsonContent.Create(changeDTO),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);

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
