using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using EventHorizon.Api.Data;
using EventHorizon.Api.Models;
using EventHorizon.Api.Tests.Helpers;

namespace EventHorizon.Api.Tests.Data;

public class EventHorizonDbContextTests : IDisposable
{
    private readonly EventHorizonDbContext _context;

    public EventHorizonDbContextTests()
    {
        _context = TestDatabaseHelper.CreateInMemoryContext(Guid.NewGuid().ToString());
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CanCreateAndRetrieveEvent()
    {
        // Arrange
        var eventItem = new Event
        {
            Id = "test-db-event",
            Title = "Database Test Event",
            Description = "Testing database operations",
            Category = "Testing",
            Date = DateTime.UtcNow.AddDays(5),
            Location = "Test Location",
            ImageUrl = "https://example.com/test.jpg"
        };

        // Act
        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedEvent = await _context.Events.FindAsync("test-db-event");
        retrievedEvent.Should().NotBeNull();
        retrievedEvent!.Title.Should().Be("Database Test Event");
        retrievedEvent.Category.Should().Be("Testing");
    }

    [Fact]
    public async Task CanCreateAndRetrieveUser()
    {
        // Arrange
        var user = new User
        {
            Id = "test-db-user",
            Username = "dbtestuser",
            Password = "testpassword"
        };

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedUser = await _context.Users.FindAsync("test-db-user");
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Username.Should().Be("dbtestuser");
        retrievedUser.Password.Should().Be("testpassword");
    }

    [Fact]
    public async Task CanQueryEventsByCategory()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var technologyEvents = await _context.Events
            .Where(e => e.Category.ToLower() == "technology")
            .ToListAsync();

        // Assert
        technologyEvents.Should().HaveCount(2);
        technologyEvents.All(e => e.Category.Equals("Technology", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }

    [Fact]
    public async Task CanQueryEventsByDateRange()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(20);

        // Act
        var eventsInRange = await _context.Events
            .Where(e => e.Date >= startDate && e.Date <= endDate)
            .ToListAsync();

        // Assert
        eventsInRange.Should().HaveCount(1);
        eventsInRange.First().Title.Should().Be("Test Event 2");
    }

    [Fact]
    public async Task CanQueryUsersByUsername()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == "testuser1");

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be("test-user-1");
        user.Username.Should().Be("testuser1");
    }

    [Fact]
    public async Task CanUpdateEvent()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var eventToUpdate = await _context.Events.FindAsync("test-event-1");
        
        // Act
        eventToUpdate!.Title = "Updated Title";
        eventToUpdate.Description = "Updated Description";
        await _context.SaveChangesAsync();

        // Assert
        var updatedEvent = await _context.Events.FindAsync("test-event-1");
        updatedEvent!.Title.Should().Be("Updated Title");
        updatedEvent.Description.Should().Be("Updated Description");
        updatedEvent.Category.Should().Be("Technology"); // Should remain unchanged
    }

    [Fact]
    public async Task CanDeleteEvent()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var eventToDelete = await _context.Events.FindAsync("test-event-1");

        // Act
        _context.Events.Remove(eventToDelete!);
        await _context.SaveChangesAsync();

        // Assert
        var deletedEvent = await _context.Events.FindAsync("test-event-1");
        deletedEvent.Should().BeNull();

        // Verify other events still exist
        var remainingEvents = await _context.Events.ToListAsync();
        remainingEvents.Should().HaveCount(2);
    }

    [Fact]
    public async Task EventDatesAreStoredAsUtc()
    {
        // Arrange
        var utcDate = DateTime.UtcNow.AddDays(10);
        var eventItem = new Event
        {
            Title = "UTC Test Event",
            Description = "Testing UTC date storage",
            Category = "Testing",
            Date = utcDate,
            Location = "UTC Test Location"
        };

        // Act
        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedEvent = await _context.Events.FindAsync(eventItem.Id);
        retrievedEvent!.Date.Kind.Should().Be(DateTimeKind.Utc);
        retrievedEvent.Date.Should().Be(utcDate);
    }

    [Fact]
    public async Task EventsAreOrderedByDateDescending()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var events = await _context.Events
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        // Assert
        events.Should().HaveCount(3);
        events.Should().BeInDescendingOrder(e => e.Date);
        events.First().Title.Should().Be("Test Event 3"); // Latest date (day 21)
        events.Last().Title.Should().Be("Test Event 1");  // Earliest date (day 7)
    }
}
