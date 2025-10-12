using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using EventHorizon.Api.Data;
using EventHorizon.Api.Models;
using EventHorizon.Api.Tests.Helpers;

namespace EventHorizon.Api.Tests.Integration;

public class EventsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public EventsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetEvents_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/events");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().Should().Contain("application/json");

        var content = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<Event[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        events.Should().HaveCount(3);
        events.Should().BeInDescendingOrder(e => e.Date);
    }

    [Fact]
    public async Task GetEvents_WithDateFilters_ReturnsFilteredResults()
    {
        // Arrange
        await SeedDatabaseAsync();
        var startDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-ddTHH:mm:ssZ");

        // Act
        var response = await _client.GetAsync($"/api/events?startDate={startDate}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<Event[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        events.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEvent_WithValidId_ReturnsEvent()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/events/test-event-1");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var eventItem = JsonSerializer.Deserialize<Event>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventItem.Should().NotBeNull();
        eventItem!.Id.Should().Be("test-event-1");
        eventItem.Title.Should().Be("Test Event 1");
    }

    [Fact]
    public async Task GetEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/events/non-existent-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateEvent_WithValidRequest_ReturnsCreatedEvent()
    {
        // Arrange
        var newEvent = new CreateEventRequest
        {
            Title = "Integration Test Event",
            Description = "Test event created during integration test",
            Category = "Testing",
            Date = DateTime.UtcNow.AddDays(30),
            Location = "Test Location",
            ImageUrl = "https://example.com/test-image.jpg"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", newEvent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<Event>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        createdEvent.Should().NotBeNull();
        createdEvent!.Title.Should().Be(newEvent.Title);
        createdEvent.Description.Should().Be(newEvent.Description);
        createdEvent.Category.Should().Be(newEvent.Category);
        createdEvent.Location.Should().Be(newEvent.Location);
        createdEvent.ImageUrl.Should().Be(newEvent.ImageUrl);
        createdEvent.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateEvent_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var invalidEvent = new
        {
            Title = "", // Invalid: empty title
            Description = "Test description",
            Category = "Testing",
            Date = DateTime.UtcNow.AddDays(30),
            Location = "Test Location"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", invalidEvent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateEvent_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        await SeedDatabaseAsync();
        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Event Title",
            Description = "Updated description"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/events/test-event-1", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update by getting the event
        var getResponse = await _client.GetAsync("/api/events/test-event-1");
        var content = await getResponse.Content.ReadAsStringAsync();
        var updatedEvent = JsonSerializer.Deserialize<Event>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        updatedEvent.Should().NotBeNull();
        updatedEvent!.Title.Should().Be("Updated Event Title");
        updatedEvent.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();
        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Title"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/events/non-existent-id", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEvent_WithValidId_ReturnsNoContent()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.DeleteAsync("/api/events/test-event-1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the event was deleted
        var getResponse = await _client.GetAsync("/api/events/test-event-1");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEvent_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.DeleteAsync("/api/events/non-existent-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEventsByCategory_WithValidCategory_ReturnsFilteredEvents()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/events/category/Technology");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<Event[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        events.Should().HaveCount(2);
        events.All(e => e.Category.Equals("Technology", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task GetEventsByCategory_WithNonExistentCategory_ReturnsEmptyArray()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/events/category/NonExistentCategory");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<Event[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        events.Should().BeEmpty();
    }

    [Fact]
    public async Task EventsCrudWorkflow_CompleteScenario_WorksCorrectly()
    {
        // This test demonstrates a complete CRUD workflow
        
        // 1. Create an event
        var newEvent = new CreateEventRequest
        {
            Title = "CRUD Test Event",
            Description = "Testing complete CRUD workflow",
            Category = "Testing",
            Date = DateTime.UtcNow.AddDays(30),
            Location = "CRUD Test Location",
            ImageUrl = "https://example.com/crud-test.jpg"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/events", newEvent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<Event>(createdContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        var eventId = createdEvent!.Id;

        // 2. Read the created event
        var readResponse = await _client.GetAsync($"/api/events/{eventId}");
        readResponse.EnsureSuccessStatusCode();

        // 3. Update the event
        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated CRUD Test Event",
            Description = "Updated description for CRUD test"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/events/{eventId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 4. Verify the update
        var verifyResponse = await _client.GetAsync($"/api/events/{eventId}");
        var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
        var updatedEvent = JsonSerializer.Deserialize<Event>(verifyContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        updatedEvent!.Title.Should().Be("Updated CRUD Test Event");

        // 5. Delete the event
        var deleteResponse = await _client.DeleteAsync($"/api/events/{eventId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verify the deletion
        var finalGetResponse = await _client.GetAsync($"/api/events/{eventId}");
        finalGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
