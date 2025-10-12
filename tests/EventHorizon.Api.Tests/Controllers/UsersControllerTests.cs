using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;
using EventHorizon.Api.Controllers;
using EventHorizon.Api.Data;
using EventHorizon.Api.Models;
using EventHorizon.Api.Tests.Helpers;

namespace EventHorizon.Api.Tests.Controllers;

public class UsersControllerTests : IDisposable
{
    private readonly EventHorizonDbContext _context;
    private readonly UsersController _controller;
    private readonly ILogger<UsersController> _logger;

    public UsersControllerTests()
    {
        _context = TestDatabaseHelper.CreateInMemoryContext(Guid.NewGuid().ToString());
        _logger = TestDatabaseHelper.CreateMockLogger<UsersController>();
        _controller = new UsersController(_context, _logger);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterUser_WithValidRequest_CreatesUserAndReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "newuser",
            Password = "password123"
        };

        // Act
        var result = await _controller.RegisterUser(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var userResponse = actionResult.Value.Should().BeOfType<UserResponse>().Subject;
        
        userResponse.Username.Should().Be(request.Username);
        userResponse.Id.Should().NotBeNullOrEmpty();
        userResponse.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));

        // Verify user was actually saved to database
        var savedUser = await _context.Users.FindAsync(userResponse.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be(request.Username);
    }

    [Fact]
    public async Task RegisterUser_WithExistingUsername_ReturnsBadRequest()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var request = new RegisterUserRequest
        {
            Username = "testuser1", // This username already exists in test data
            Password = "password123"
        };

        // Act
        var result = await _controller.RegisterUser(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = actionResult.Value.Should().BeAssignableTo<object>().Subject;
        
        // Verify the error message
        var errorMessage = errorResponse.GetType().GetProperty("message")?.GetValue(errorResponse)?.ToString();
        errorMessage.Should().Contain("Username already exists");
    }

    [Fact]
    public async Task LoginUser_WithExistingUserAndCorrectPassword_ReturnsLoginResponse()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var request = new LoginUserRequest
        {
            Username = "testuser1",
            Password = "password123"
        };

        // Act
        var result = await _controller.LoginUser(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var loginResponse = actionResult.Value.Should().BeOfType<LoginResponse>().Subject;
        
        loginResponse.Username.Should().Be(request.Username);
        loginResponse.Id.Should().Be("test-user-1");
        loginResponse.IsNewUser.Should().BeFalse();
        loginResponse.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task LoginUser_WithExistingUserAndIncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var request = new LoginUserRequest
        {
            Username = "testuser1",
            Password = "wrongpassword"
        };

        // Act
        var result = await _controller.LoginUser(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorResponse = actionResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var errorMessage = errorResponse.GetType().GetProperty("message")?.GetValue(errorResponse)?.ToString();
        errorMessage.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task LoginUser_WithNewUser_CreatesUserAndReturnsLoginResponse()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var request = new LoginUserRequest
        {
            Username = "newuser",
            Password = "password123"
        };

        // Act
        var result = await _controller.LoginUser(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var loginResponse = actionResult.Value.Should().BeOfType<LoginResponse>().Subject;
        
        loginResponse.Username.Should().Be(request.Username);
        loginResponse.Id.Should().NotBeNullOrEmpty();
        loginResponse.IsNewUser.Should().BeTrue();
        loginResponse.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));

        // Verify user was created in database
        var createdUser = await _context.Users.FindAsync(loginResponse.Id);
        createdUser.Should().NotBeNull();
        createdUser!.Username.Should().Be(request.Username);
    }

    [Fact] 
    public async Task GetUser_WithValidId_ReturnsUserResponse()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.GetUser("test-user-1");

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userResponse = actionResult.Value.Should().BeOfType<UserResponse>().Subject;
        
        userResponse.Id.Should().Be("test-user-1");
        userResponse.Username.Should().Be("testuser1");
        userResponse.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task CheckUsernameAvailability_WithAvailableUsername_ReturnsTrue()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.CheckUsernameAvailability("availableusername");

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var available = response.GetType().GetProperty("available")?.GetValue(response);
        available.Should().Be(true);
    }

    [Fact]
    public async Task CheckUsernameAvailability_WithExistingUsername_ReturnsFalse()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.CheckUsernameAvailability("testuser1");

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var available = response.GetType().GetProperty("available")?.GetValue(response);
        available.Should().Be(false);
    }
}
