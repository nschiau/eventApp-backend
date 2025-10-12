using System.ComponentModel.DataAnnotations;

namespace EventHorizon.Api.Models;

public class Event
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public string Location { get; set; } = string.Empty;
    
    public string? ImageUrl { get; set; }
    
    public string? CreatedById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateEventRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public string Location { get; set; } = string.Empty;
    
    public string? ImageUrl { get; set; }
}

public class UpdateEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public DateTime? Date { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
}
