using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EventHorizon.Api.Data;

namespace EventHorizon.Api.Tests.Integration;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<EventHorizonDbContext>));
            services.RemoveAll(typeof(EventHorizonDbContext));

            // Add a database context using an in-memory database for testing
            services.AddDbContext<EventHorizonDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: "InMemoryTestDb");
            });

            // Build the service provider and seed the database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EventHorizonDbContext>();
            
            // Ensure the database is created
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}
