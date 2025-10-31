# EventApp Backend

# CD pipeline.

.NET 9 Web API backend for the EventHorizon application.

## Features

- RESTful API for event management
- Entity Framework Core with PostgreSQL
- CRUD operations for events
- Swagger documentation
- Docker containerization

## API Endpoints

- `GET /api/events` - Get all events
- `GET /api/events/{id}` - Get event by ID
- `GET /api/events/category/{category}` - Get events by category
- `POST /api/events` - Create new event
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

## Development

```bash
# Restore packages
dotnet restore

# Run the application
dotnet run

# Build the application
dotnet build

# Run tests (if any)
dotnet test
```

## Docker

```bash
# Build the container
docker build -t eventapp-backend .

# Run the container
docker run -p 8080:8080 eventapp-backend
```

## Environment Variables

- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `ASPNETCORE_URLS` - URLs to bind to
