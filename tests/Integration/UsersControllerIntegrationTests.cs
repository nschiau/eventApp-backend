using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EventHorizon.Api.Data;
using EventHorizon.Api.Tests.Helpers;

namespace EventHorizon.Api.Tests.Integration;

public class UsersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UsersControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventHorizonDbContext>();
        await TestDatabaseHelper.SeedTestDataAsync(context);
    }

    [Fact]
    public async Task RegisterUser_WithValidRequest_ReturnsCreatedUser()
    {
        // Arrange
        var registerRequest = new RegisterUserRequest
        {
            Username = "integrationtestuser",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userResponse.Should().NotBeNull();
        userResponse!.Username.Should().Be(registerRequest.Username);
        userResponse.Id.Should().NotBeNullOrEmpty();
        userResponse.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task RegisterUser_WithExistingUsername_ReturnsBadRequest()
    {
        // Arrange
        await SeedDatabaseAsync();
        var registerRequest = new RegisterUserRequest
        {
            Username = "testuser1", // Existing user in test data
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username already exists");
    }

    [Fact]
    public async Task RegisterUser_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var invalidRequest = new
        {
            Username = "", // Invalid: empty username
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/register", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginUser_WithExistingUserAndCorrectPassword_ReturnsLoginResponse()
    {
        // Arrange
        await SeedDatabaseAsync();
        var loginRequest = new LoginUserRequest
        {
            Username = "testuser1",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        loginResponse.Should().NotBeNull();
        loginResponse!.Username.Should().Be(loginRequest.Username);
        loginResponse.Id.Should().Be("test-user-1");
        loginResponse.IsNewUser.Should().BeFalse();
    }

    [Fact]
    public async Task LoginUser_WithExistingUserAndIncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange
        await SeedDatabaseAsync();
        var loginRequest = new LoginUserRequest
        {
            Username = "testuser1",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task LoginUser_WithNewUser_CreatesUserAndReturnsLoginResponse()
    {
        // Arrange
        var loginRequest = new LoginUserRequest
        {
            Username = "newintegrationuser",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        loginResponse.Should().NotBeNull();
        loginResponse!.Username.Should().Be(loginRequest.Username);
        loginResponse.Id.Should().NotBeNullOrEmpty();
        loginResponse.IsNewUser.Should().BeTrue();
        loginResponse.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUserResponse()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/users/test-user-1");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userResponse.Should().NotBeNull();
        userResponse!.Id.Should().Be("test-user-1");
        userResponse.Username.Should().Be("testuser1");
        userResponse.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/users/non-existent-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CheckUsernameAvailability_WithAvailableUsername_ReturnsTrue()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/users/check-username/availableusername");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.GetProperty("available").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CheckUsernameAvailability_WithExistingUsername_ReturnsFalse()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/users/check-username/testuser1");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.GetProperty("available").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task UserRegistrationAndLoginWorkflow_CompleteScenario_WorksCorrectly()
    {
        // This test demonstrates a complete user registration and login workflow
        
        var username = "workflowuser" + Guid.NewGuid().ToString("N")[..8];
        var password = "password123";

        // 1. Check username availability (should be available)
        var availabilityResponse = await _client.GetAsync($"/api/users/check-username/{username}");
        availabilityResponse.EnsureSuccessStatusCode();
        var availabilityContent = await availabilityResponse.Content.ReadAsStringAsync();
        var availabilityResult = JsonSerializer.Deserialize<JsonElement>(availabilityContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        availabilityResult.GetProperty("available").GetBoolean().Should().BeTrue();

        // 2. Register the user
        var registerRequest = new RegisterUserRequest
        {
            Username = username,
            Password = password
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        var userId = userResponse!.Id;

        // 3. Check username availability again (should not be available)
        var availabilityResponse2 = await _client.GetAsync($"/api/users/check-username/{username}");
        var availabilityContent2 = await availabilityResponse2.Content.ReadAsStringAsync();
        var availabilityResult2 = JsonSerializer.Deserialize<JsonElement>(availabilityContent2, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        availabilityResult2.GetProperty("available").GetBoolean().Should().BeFalse();

        // 4. Login with the registered user
        var loginRequest = new LoginUserRequest
        {
            Username = username,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        loginResult!.Username.Should().Be(username);
        loginResult.Id.Should().Be(userId);
        loginResult.IsNewUser.Should().BeFalse();

        // 5. Get user details
        var getUserResponse = await _client.GetAsync($"/api/users/{userId}");
        getUserResponse.EnsureSuccessStatusCode();

        var getUserContent = await getUserResponse.Content.ReadAsStringAsync();
        var getUserResult = JsonSerializer.Deserialize<UserResponse>(getUserContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        getUserResult!.Id.Should().Be(userId);
        getUserResult.Username.Should().Be(username);

        // 6. Try to login with wrong password (should fail)
        var wrongLoginRequest = new LoginUserRequest
        {
            Username = username,
            Password = "wrongpassword"
        };

        var wrongLoginResponse = await _client.PostAsJsonAsync("/api/users/login", wrongLoginRequest);
        wrongLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
