using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using EventHorizon.Api.Data;
using EventHorizon.Api.Models;

namespace EventHorizon.Api.Tests.Helpers;

public static class TestDatabaseHelper
{
    public static EventHorizonDbContext CreateInMemoryContext(string databaseName = "TestDatabase")
    {
        var options = new DbContextOptionsBuilder<EventHorizonDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new EventHorizonDbContext(options);
    }

    public static async Task SeedTestDataAsync(EventHorizonDbContext context)
    {
        // Clear existing data
        context.Events.RemoveRange(context.Events);
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();

        // Seed test events
        var testEvents = new List<Event>
        {
            new Event
            {
                Id = "test-event-1",
                Title = "Test Event 1",
                Description = "Description for test event 1",
                Category = "Technology",
                Date = DateTime.UtcNow.AddDays(7),
                Location = "Test Location 1",
                ImageUrl = "https://example.com/image1.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new Event
            {
                Id = "test-event-2",
                Title = "Test Event 2",
                Description = "Description for test event 2",
                Category = "Music",
                Date = DateTime.UtcNow.AddDays(14),
                Location = "Test Location 2",
                ImageUrl = "https://example.com/image2.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new Event
            {
                Id = "test-event-3",
                Title = "Test Event 3",
                Description = "Description for test event 3",
                Category = "Technology",
                Date = DateTime.UtcNow.AddDays(21),
                Location = "Test Location 3",
                ImageUrl = null,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Seed test users
        var testUsers = new List<User>
        {
            new User
            {
                Id = "test-user-1",
                Username = "testuser1",
                Password = "password123",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = "test-user-2",
                Username = "testuser2",
                Password = "password456",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Events.AddRange(testEvents);
        context.Users.AddRange(testUsers);
        await context.SaveChangesAsync();
    }

    public static ILogger<T> CreateMockLogger<T>()
    {
        var mockLogger = new Mock<ILogger<T>>();
        return mockLogger.Object;
    }
}
