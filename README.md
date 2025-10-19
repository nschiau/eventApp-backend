# EventHorizon API - Backend
# Test webhook

A modern .NET 9 Web API for event management with Entity Framework Core, PostgreSQL, and comprehensive unit testing.

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- PostgreSQL (or use Docker)
- Git

### Run with Docker (Recommended)
```bash
# Clone the repository
git clone <your-backend-repo-url>
cd eventapp-backend

# Start with Docker Compose (includes PostgreSQL)
docker-compose up --build
```
- **API**: http://localhost:8080
- **Swagger Documentation**: http://localhost:8080/swagger

### Local Development
```bash
# Install .NET 9 SDK if not already installed
# Clone and navigate to project
git clone <your-backend-repo-url>
cd eventapp-backend

# Restore packages
dotnet restore

# Run the API
dotnet run --project src/EventHorizon.Api
```

## 📁 Project Structure

```
eventapp-backend/
├── src/
│   └── EventHorizon.Api/              # Main API project
│       ├── Controllers/               # API Controllers
│       │   ├── EventsController.cs   # Events CRUD operations
│       │   └── UsersController.cs    # User management
│       ├── Data/                      # Entity Framework
│       │   └── EventHorizonDbContext.cs # Database context
│       ├── Models/                    # Data models & DTOs
│       │   ├── Event.cs              # Event entity
│       │   ├── User.cs               # User entity
│       │   └── UserDtos.cs           # User request/response DTOs
│       ├── Properties/               # Launch settings
│       ├── Program.cs                # Application entry point
│       ├── appsettings.json          # Configuration
│       ├── Dockerfile                # Container definition
│       └── EventHorizon.Api.csproj   # Project file
├── tests/
│   └── EventHorizon.Api.Tests/        # Unit test suite
│       ├── Controllers/               # Controller tests (15 tests)
│       ├── Helpers/                   # Test utilities
│       └── EventHorizon.Api.Tests.csproj
├── docker-compose.yml                # PostgreSQL + API
├── init-db.sql                       # Database initialization
├── EventHorizon.sln                  # Solution file
└── README.md                         # This file
```

## 🔧 Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core**: ORM for database operations
- **PostgreSQL**: Primary database
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit testing framework
- **FluentAssertions**: Readable test assertions
- **Docker**: Containerization

## 📊 API Endpoints

### Events
- `GET /api/events` - Get all events (with optional date filtering)
- `GET /api/events/{id}` - Get event by ID
- `GET /api/events/category/{category}` - Get events by category
- `POST /api/events` - Create new event
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

### Users
- `POST /api/users/register` - Register new user
- `POST /api/users/login` - User login (creates user if not exists)
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/check-username/{username}` - Check username availability

## 🧪 Testing

The project includes comprehensive unit tests covering all API endpoints.

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific controller tests
dotnet test --filter "FullyQualifiedName~EventsController"
dotnet test --filter "FullyQualifiedName~UsersController"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage
- **Total Tests**: 15
- **Events API**: 7 tests (CRUD operations, filtering, error handling)
- **Users API**: 8 tests (registration, login, validation, availability)
- **Test Features**: In-memory database, isolated tests, comprehensive assertions

## 🐳 Docker Setup

### Development with Docker Compose
```bash
# Start PostgreSQL + API
docker-compose up --build

# Stop services
docker-compose down

# View logs
docker-compose logs -f api
```

### Production Docker Build
```bash
# Build API image
docker build -t eventhorizon-api -f src/EventHorizon.Api/Dockerfile .

# Run with external PostgreSQL
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=your-db;Database=eventhorizon;Username=user;Password=pass" \
  eventhorizon-api
```

## ⚙️ Configuration

### Database Connection
Configure in `src/EventHorizon.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=eventhorizon;Username=postgres;Password=postgres;Port=5432"
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ASPNETCORE_URLS`: Binding URLs
- `ConnectionStrings__DefaultConnection`: Database connection string

### CORS Configuration
Currently configured for frontend at:
- http://localhost:3000 (React dev server)
- http://localhost:3001 (Docker frontend)
- http://localhost:5173 (Vite dev server)

## 🚀 Development Workflow

### Adding New Features
1. **Create Model**: Add to `src/EventHorizon.Api/Models/`
2. **Update DbContext**: Modify `EventHorizonDbContext.cs`
3. **Create Controller**: Add to `src/EventHorizon.Api/Controllers/`
4. **Write Tests**: Add tests to `tests/EventHorizon.Api.Tests/Controllers/`
5. **Run Tests**: `dotnet test`
6. **Update Documentation**: Update this README

### Database Migrations
```bash
cd src/EventHorizon.Api

# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Adding Packages
```bash
# Add to API project
dotnet add src/EventHorizon.Api package PackageName

# Add to test project
dotnet add tests/EventHorizon.Api.Tests package PackageName
```

## 📝 API Examples

### Create Event
```bash
curl -X POST http://localhost:8080/api/events \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Tech Conference 2025",
    "description": "Annual technology conference",
    "category": "Technology",
    "date": "2025-12-01T18:00:00Z",
    "location": "Conference Center",
    "imageUrl": "https://example.com/image.jpg"
  }'
```

### Get Events by Category
```bash
curl http://localhost:8080/api/events/category/Technology
```

### Register User
```bash
curl -X POST http://localhost:8080/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "password": "securepassword"
  }'
```

## 🔒 Security Notes

⚠️ **Important**: This is a development version with basic security:
- Passwords are stored in plain text (use hashing in production)
- No authentication tokens (implement JWT in production)
- Basic CORS configuration (restrict in production)
- No rate limiting (add in production)

### Production Recommendations
- Implement password hashing (bcrypt)
- Add JWT authentication
- Use HTTPS only
- Implement rate limiting
- Add input validation
- Use secrets management
- Enable detailed logging

## 🐛 Troubleshooting

### Common Issues

**Build Errors**
```bash
# Clear build artifacts
dotnet clean
dotnet restore
dotnet build
```

**Database Connection Issues**
- Ensure PostgreSQL is running
- Check connection string in `appsettings.json`
- Verify database exists

**Port Already in Use**
```bash
# Kill process on port 8080
netstat -ano | findstr :8080
taskkill /PID <PID> /F
```

**Test Failures**
```bash
# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## 📚 Additional Resources

- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [xUnit Testing](https://xunit.net/)
- [Docker Documentation](https://docs.docker.com/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Write tests for your changes
4. Ensure all tests pass: `dotnet test`
5. Commit your changes: `git commit -m 'Add amazing feature'`
6. Push to the branch: `git push origin feature/amazing-feature`
7. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🚀 Deployment

### Azure App Service
1. Create App Service with .NET 9
2. Configure connection string in Application Settings
3. Deploy using GitHub Actions or Azure CLI

### AWS Elastic Beanstalk
1. Create .NET environment
2. Package application: `dotnet publish -c Release`
3. Deploy ZIP file

### Heroku
1. Add Heroku PostgreSQL addon
2. Configure buildpacks for .NET
3. Deploy via Git push

---

**Version**: 1.0.0  
**Last Updated**: October 2025
