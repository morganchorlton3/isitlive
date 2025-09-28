using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IsItLive.Api.Models;

namespace IsItLive.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory for testing that configures an in-memory database
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove all existing DbContext registrations more thoroughly
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                           d.ServiceType == typeof(AppDbContext) ||
                           d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database with a unique name for each test
            var databaseName = $"TestDatabase_{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }, ServiceLifetime.Scoped);
        });
    }
}
