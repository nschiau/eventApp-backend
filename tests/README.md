# EventHorizon API Unit Tests

This directory contains comprehensive unit and integration tests for the EventHorizon API endpoints.

## Test Structure

```
Tests/
├── Controllers/                  # Unit tests for API controllers
│   ├── EventsControllerTests.cs    # Tests for Events API endpoints
│   └── UsersControllerTests.cs     # Tests for Users API endpoints
├── Integration/                  # Integration tests
│   ├── CustomWebApplicationFactory.cs    # Test server setup
│   ├── EventsControllerIntegrationTests.cs  # Full HTTP pipeline tests for Events
│   └── UsersControllerIntegrationTests.cs   # Full HTTP pipeline tests for Users
├── Data/                        # Database layer tests
│   └── EventHorizonDbContextTests.cs   # Entity Framework tests
└── Helpers/                     # Test utilities
    └── TestDatabaseHelper.cs       # Database seeding and mock utilities
```

## Test Coverage

### EventsController Tests
- ✅ **GET /api/events** - Get all events with optional date filtering
- ✅ **GET /api/events/{id}** - Get specific event by ID
- ✅ **POST /api/events** - Create new event
- ✅ **PUT /api/events/{id}** - Update existing event
- ✅ **DELETE /api/events/{id}** - Delete event
- ✅ **GET /api/events/category/{category}** - Get events by category with filtering

### UsersController Tests
- ✅ **POST /api/users/register** - User registration
- ✅ **POST /api/users/login** - User login (existing and new users)
- ✅ **GET /api/users/{id}** - Get user by ID
- ✅ **GET /api/users/check-username/{username}** - Check username availability

### Database Tests
- ✅ Entity creation and retrieval
- ✅ Query operations (filtering, ordering)
- ✅ Update and delete operations
- ✅ UTC date handling
- ✅ Case-insensitive username queries

## Test Features

### Unit Tests
- **In-Memory Database**: Each test uses an isolated in-memory database
- **Mock Logging**: Proper logger mocking for controller dependencies
- **Comprehensive Assertions**: FluentAssertions for readable test assertions
- **Edge Case Coverage**: Tests for invalid inputs, not found scenarios, etc.

### Integration Tests
- **Full HTTP Pipeline**: Tests complete request/response cycle
- **Custom Test Server**: Isolated test environment with in-memory database
- **JSON Serialization**: Proper handling of API request/response formats
- **Status Code Validation**: HTTP status code assertions
- **End-to-End Workflows**: Complete CRUD operation scenarios

## Running the Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test --filter "FullyQualifiedName~EventHorizon.Api.Tests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in specific namespace
dotnet test --filter "FullyQualifiedName~Controllers"
dotnet test --filter "FullyQualifiedName~Integration"

# Run specific test method
dotnet test --filter "Method=GetEvents_WithNoFilters_ReturnsAllEventsOrderedByDateDescending"
```

### Visual Studio / VS Code
- Use the Test Explorer to run individual tests or test suites
- Debug tests by setting breakpoints and using "Debug Test"
- View test results and coverage in the Test Output window

## Test Data

### Sample Events
- **Test Event 1**: Technology category, 7 days from now
- **Test Event 2**: Music category, 14 days from now  
- **Test Event 3**: Technology category, 21 days from now

### Sample Users
- **testuser1**: ID "test-user-1", password "password123"
- **testuser2**: ID "test-user-2", password "password456"

## Test Patterns

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and dependencies
    await TestDatabaseHelper.SeedTestDataAsync(_context);
    var request = new CreateEventRequest { /* ... */ };

    // Act - Execute the method under test
    var result = await _controller.CreateEvent(request);

    // Assert - Verify the results
    var actionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
    var createdEvent = actionResult.Value.Should().BeOfType<Event>().Subject;
    createdEvent.Title.Should().Be(request.Title);
}
```

### Test Method Naming
Format: `MethodName_Scenario_ExpectedResult`
- `GetEvents_WithNoFilters_ReturnsAllEventsOrderedByDateDescending`
- `CreateEvent_WithValidRequest_CreatesEventAndReturnsCreatedResult`
- `LoginUser_WithIncorrectPassword_ReturnsUnauthorized`

## Testing Dependencies

### Core Testing Packages
- **xUnit**: Primary testing framework
- **FluentAssertions**: Readable assertion library
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Moq**: Mocking framework for dependencies

### Test-Specific Configurations
- **In-Memory Database**: Isolated database per test to prevent interference
- **Mock Logger**: Prevents logging noise during test execution
- **Custom WebApplicationFactory**: Configures test server with test dependencies

## Best Practices Implemented

1. **Test Isolation**: Each test has its own database instance
2. **Descriptive Test Names**: Clear indication of what is being tested
3. **Comprehensive Coverage**: Happy path, edge cases, and error scenarios
4. **Fast Execution**: In-memory databases for quick test runs
5. **Readable Assertions**: FluentAssertions for clear test failure messages
6. **Proper Cleanup**: IDisposable implementation for resource cleanup
7. **Realistic Test Data**: Representative sample data for various scenarios

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- No external dependencies (uses in-memory database)
- Fast execution (typically under 30 seconds for full suite)
- Deterministic results (no time-dependent or random elements)
- Clear failure reporting with detailed assertion messages

## Extending the Tests

When adding new API endpoints or functionality:

1. **Add Unit Tests**: Create test methods in the appropriate controller test class
2. **Add Integration Tests**: Add HTTP-level tests for new endpoints
3. **Update Test Data**: Modify `TestDatabaseHelper.SeedTestDataAsync` if needed
4. **Follow Naming Conventions**: Use the established naming patterns
5. **Test Edge Cases**: Include both success and failure scenarios
6. **Update Documentation**: Add new test coverage to this README
