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
        events.First().Title.Should().Be("Test Event 3"); // Latest date
    }

    [Fact]
    public async Task GetEvents_WithStartDateFilter_ReturnsFilteredEvents()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var filterDate = DateTime.UtcNow.AddDays(10);

        // Act
        var result = await _controller.GetEvents(startDate: filterDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = actionResult.Value.Should().BeAssignableTo<IEnumerable<Event>>().Subject.ToList();
        
        events.Should().HaveCount(2); // Only events after day 10
        events.All(e => e.Date >= filterDate).Should().BeTrue();
    }

    [Fact]
    public async Task GetEvents_WithEndDateFilter_ReturnsFilteredEvents()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var filterDate = DateTime.UtcNow.AddDays(15);

        // Act
        var result = await _controller.GetEvents(endDate: filterDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = actionResult.Value.Should().BeAssignableTo<IEnumerable<Event>>().Subject.ToList();
        
        events.Should().HaveCount(2); // Only events before day 15
        events.All(e => e.Date <= filterDate).Should().BeTrue();
    }

    [Fact]
    public async Task GetEvents_WithStartAndEndDateFilter_ReturnsFilteredEvents()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(15);

        // Act
        var result = await _controller.GetEvents(startDate: startDate, endDate: endDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = actionResult.Value.Should().BeAssignableTo<IEnumerable<Event>>().Subject.ToList();
        
        events.Should().HaveCount(1); // Only event on day 14
        events.All(e => e.Date >= startDate && e.Date <= endDate).Should().BeTrue();
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
    public async Task UpdateEvent_WithPartialRequest_UpdatesOnlyProvidedFields()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var updateRequest = new UpdateEventRequest
        {
            Title = "Only Title Updated"
            // Other fields are null/not provided
        };

        // Act
        var result = await _controller.UpdateEvent("test-event-1", updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify only title was updated
        var updatedEvent = await _context.Events.FindAsync("test-event-1");
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Title.Should().Be("Only Title Updated");
        updatedEvent.Description.Should().Be("Description for test event 1"); // Unchanged
        updatedEvent.Category.Should().Be("Technology"); // Unchanged
    }

    [Fact]
    public async Task UpdateEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var updateRequest = new UpdateEventRequest { Title = "Updated Title" };

        // Act
        var result = await _controller.UpdateEvent("non-existent-id", updateRequest);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
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
    public async Task DeleteEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.DeleteEvent("non-existent-id");

        // Assert
        result.Should().BeOfType<NotFoundResult>();

        // Verify no events were deleted
        var events = _context.Events.ToList();
        events.Should().HaveCount(3);
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

    [Fact]
    public async Task GetEventsByCategory_WithCaseInsensitiveCategory_ReturnsFilteredEvents()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.GetEventsByCategory("tEcHnOlOgY");

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = actionResult.Value.Should().BeAssignableTo<IEnumerable<Event>>().Subject.ToList();
        
        events.Should().HaveCount(2);
        events.All(e => e.Category.Equals("Technology", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task GetEventsByCategory_WithNonExistentCategory_ReturnsEmptyList()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);

        // Act
        var result = await _controller.GetEventsByCategory("NonExistentCategory");

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = actionResult.Value.Should().BeAssignableTo<IEnumerable<Event>>().Subject.ToList();
        
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEventsByCategory_WithDateFilters_ReturnsFilteredEvents()
    {
        // Arrange
        await TestDatabaseHelper.SeedTestDataAsync(_context);
        var startDate = DateTime.UtcNow.AddDays(20); // Only the third event should match

        // Act
        var result = await _controller.GetEventsByCategory("Technology", startDate: startDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = actionResult.Value.Should().BeAssignableTo<IEnumerable<Event>>().Subject.ToList();
        
        events.Should().HaveCount(1);
        events.First().Title.Should().Be("Test Event 3");
        events.All(e => e.Category.Equals("Technology", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        events.All(e => e.Date >= startDate).Should().BeTrue();
    }
}
