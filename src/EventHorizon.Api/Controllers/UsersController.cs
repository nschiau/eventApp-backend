using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHorizon.Api.Data;
using EventHorizon.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace EventHorizon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly EventHorizonDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(EventHorizonDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST: api/Users/register
    [HttpPost("register")]
    public async Task<ActionResult<User>> RegisterUser(RegisterUserRequest request)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (existingUser != null)
            {
                return BadRequest(new { message = "Username already exists. Please choose a different username." });
            }

            var user = new User
            {
                Username = request.Username,
                Password = request.Password // In production, this should be hashed
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return user without password
            var userResponse = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
        {
            // Handle database-level unique constraint violation
            _logger.LogWarning("Attempted to create duplicate username: {Username}", request.Username);
            return BadRequest(new { message = "Username already exists. Please choose a different username." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user with username {Username}", request.Username);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Users/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LoginUser(LoginUserRequest request)
    {
        try
        {
            // First, find the user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user == null)
            {
                // User doesn't exist, create a new one
                _logger.LogInformation("Creating new user during login: {Username}", request.Username);
                
                var newUser = new User
                {
                    Username = request.Username,
                    Password = request.Password // In production, this should be hashed
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                var newUserResponse = new LoginResponse
                {
                    Id = newUser.Id,
                    Username = newUser.Username,
                    CreatedAt = newUser.CreatedAt,
                    IsNewUser = true
                };

                return Ok(newUserResponse);
            }

            // User exists, validate the password
            if (user.Password != request.Password)
            {
                _logger.LogWarning("Login attempt with incorrect password for username: {Username}", request.Username);
                return Unauthorized(new { message = "Invalid username or password." });
            }

            _logger.LogInformation("Successful login for existing user: {Username}", user.Username);

            var userResponse = new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                IsNewUser = false
            };

            return Ok(userResponse);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
        {
            // Handle race condition where user was created between our check and insert
            _logger.LogWarning("Race condition detected during user creation for username: {Username}", request.Username);
            return await LoginUser(request); // Retry the login
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username {Username}", request.Username);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(string id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userResponse = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };

            return Ok(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with id {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Users/check-username/{username}
    [HttpGet("check-username/{username}")]
    public async Task<ActionResult<bool>> CheckUsernameAvailability(string username)
    {
        try
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Username.ToLower() == username.ToLower());

            return Ok(new { available = !exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability for {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
}
