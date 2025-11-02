using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;
using EventHorizon.Api.Controllers;
using EventHorizon.Api.Data;
using EventHorizon.Api.Models;
using EventHorizon.Api.Tests.Helpers;

namespace EventHorizon.Api.Tests.Controllers;

public class EventsControllerTests : IDisposable
{
    private readonly EventHorizonDbContext _context;
    private readonly EventsController _controller;
    private readonly ILogger<EventsController> _logger;

    public EventsControllerTests()
    {
        _context = TestDatabaseHelper.CreateInMemoryContext(Guid.NewGuid().ToString());
        _logger = TestDatabaseHelper.CreateMockLogger<EventsController>();
        _controller = new EventsController(_context, _logger);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetEvents_WithNoFilters_ReturnsAllEventsOrderedByDateDescending()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.GetEvents();

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = actionResult.Value.Should().BeAssignableTo<IEnumerable<Event>>().Subject.ToList();

        events.Should().HaveCount(3);
        events.Should().BeInDescendingOrder(e => e.Date);
        events.First().Title.Should().Be("Test Event 3");
    }

    [Fact]
    public async Task GetEvent_WithValidId_ReturnsEvent()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.GetEvent("test-event-1");

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var eventItem = actionResult.Value.Should().BeOfType<Event>().Subject;

        eventItem.Id.Should().Be("test-event-1");
        eventItem.Title.Should().Be("Test Event 1");
    }

    [Fact]
    public async Task GetEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.GetEvent("non-existent-id");

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateEvent_WithValidRequest_CreatesEventAndReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateEventRequest
        {
            Title = "New Test Event",
            Description = "New event description",
            Category = "Sports",
            Date = DateTime.UtcNow.AddDays(30),
            Location = "New Location",
            ImageUrl = "https://example.com/new-image.jpg"
        };

        // Act
        var result = await _controller.CreateEvent(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var createdEvent = actionResult.Value.Should().BeOfType<Event>().Subject;

        createdEvent.Title.Should().Be(request.Title);
        createdEvent.Description.Should().Be(request.Description);
        createdEvent.Category.Should().Be(request.Category);
        createdEvent.Date.Should().Be(request.Date);
        createdEvent.Location.Should().Be(request.Location);
        createdEvent.ImageUrl.Should().Be(request.ImageUrl);
        createdEvent.Id.Should().NotBeNullOrEmpty();
        createdEvent.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));

        // Verify event was actually saved to database
        var savedEvent = await _context.Events.FindAsync(createdEvent.Id);
        savedEvent.Should().NotBeNull();
        savedEvent!.Title.Should().Be(request.Title);
    }

    [Fact]
    public async Task UpdateEvent_WithValidIdAndRequest_UpdatesEventAndReturnsNoContent()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Category = "Updated Category"
        };

        // Act
        var result = await _controller.UpdateEvent("test-event-1", updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify the event was updated
        var updatedEvent = await _context.Events.FindAsync("test-event-1");
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Title.Should().Be("Updated Title");
        updatedEvent.Description.Should().Be("Updated Description");
        updatedEvent.Category.Should().Be("Updated Category");
        // Original location should remain unchanged
        updatedEvent.Location.Should().Be("Test Location 1");
    }

    [Fact]
    public async Task DeleteEvent_WithValidId_DeletesEventAndReturnsNoContent()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.DeleteEvent("test-event-1");

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify the event was deleted
        var deletedEvent = await _context.Events.FindAsync("test-event-1");
        deletedEvent.Should().BeNull();

        // Verify other events still exist
        var remainingEvents = _context.Events.ToList();
        remainingEvents.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventsByCategory_WithValidCategory_ReturnsFilteredEvents()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.GetEventsByCategory("Technology");

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = actionResult.Value.Should().BeAssignableTo<IEnumerable<Event>>().Subject.ToList();

        events.Should().HaveCount(2);
        events.All(e => e.Category.Equals("Technology", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        events.Should().BeInDescendingOrder(e => e.Date);
    }
}
