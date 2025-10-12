using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHorizon.Api.Data;
using EventHorizon.Api.Models;

namespace EventHorizon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly EventHorizonDbContext _context;
    private readonly ILogger<EventsController> _logger;

    public EventsController(EventHorizonDbContext context, ILogger<EventsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Events
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = _context.Events.AsQueryable();

            if (startDate.HasValue)
            {
                var utcStartDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                query = query.Where(e => e.Date >= utcStartDate);
            }

            if (endDate.HasValue)
            {
                var utcEndDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
                query = query.Where(e => e.Date <= utcEndDate);
            }

            var events = await query
                .OrderByDescending(e => e.Date)
                .ToListAsync();
            
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events with startDate: {StartDate}, endDate: {EndDate}. Error: {Error}", startDate, endDate, ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Events/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEvent(string id)
    {
        try
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event with id {EventId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Events
    [HttpPost]
    public async Task<ActionResult<Event>> CreateEvent(CreateEventRequest request)
    {
        try
        {
            var eventItem = new Event
            {
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                Date = request.Date,
                Location = request.Location,
                ImageUrl = request.ImageUrl
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = eventItem.Id }, eventItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/Events/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(string id, UpdateEventRequest request)
    {
        try
        {
            var eventItem = await _context.Events.FindAsync(id);
            
            if (eventItem == null)
            {
                return NotFound();
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Title))
                eventItem.Title = request.Title;
            
            if (!string.IsNullOrEmpty(request.Description))
                eventItem.Description = request.Description;
            
            if (!string.IsNullOrEmpty(request.Category))
                eventItem.Category = request.Category;
            
            if (request.Date.HasValue)
                eventItem.Date = request.Date.Value;
            
            if (!string.IsNullOrEmpty(request.Location))
                eventItem.Location = request.Location;
            
            if (request.ImageUrl != null)
                eventItem.ImageUrl = request.ImageUrl;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event with id {EventId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/Events/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        try
        {
            var eventItem = await _context.Events.FindAsync(id);
            
            if (eventItem == null)
            {
                return NotFound();
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event with id {EventId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Events/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetEventsByCategory(
        string category,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = _context.Events
                .Where(e => e.Category.ToLower() == category.ToLower());

            if (startDate.HasValue)
            {
                var utcStartDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                query = query.Where(e => e.Date >= utcStartDate);
            }

            if (endDate.HasValue)
            {
                var utcEndDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
                query = query.Where(e => e.Date <= utcEndDate);
            }

            var events = await query
                .OrderByDescending(e => e.Date)
                .ToListAsync();
            
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for category {Category} with startDate: {StartDate}, endDate: {EndDate}. Error: {Error}", category, startDate, endDate, ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}
