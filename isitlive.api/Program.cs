using IsItLive.Api.Models;
using MonitorEntity = IsItLive.Api.Models.Monitor;
using Microsoft.EntityFrameworkCore;
using Monitor = System.Threading.Monitor;

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure pipeline
ConfigurePipeline(app);

// Ensure database is migrated (skip in test environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}

app.Run();

// Make Program class accessible for testing
public partial class Program
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment = null)
    {
        // CORS: allow your Blazor app (adjust port if needed)
        var blazorOrigin = configuration["BlazorOrigin"] ?? "http://localhost:5001";
        services.AddCors(o =>
        {
            o.AddDefaultPolicy(p => p
                .WithOrigins(blazorOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        // Swagger (nice while learning)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // JSON configuration
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null; // Use exact property names
        });

        // Shared HttpClient
        services.AddHttpClient("monitor");

        // Only register PostgreSQL in non-testing environments
        var env = environment?.EnvironmentName ?? configuration["ASPNETCORE_ENVIRONMENT"];
        if (env != "Testing")
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseNpgsql(configuration.GetConnectionString("Postgres")));
        }
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors();

        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint("/swagger/v1/swagger.json", "IsItLive API v1");
            o.RoutePrefix = "swagger";
        });

        // Redirect root → Swagger
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

        app.MapGet("/health", () => Results.Ok(new { status = "ok", ts = DateTimeOffset.UtcNow }));

        // List
        app.MapGet("/monitors", async (AppDbContext db) =>
            await db.Monitors.AsNoTracking().OrderBy(m => m.Name).ToListAsync());

        app.MapPost("/monitors", async (AppDbContext db, MonitorEntity m) =>
        {
            m.Id = Guid.NewGuid();
            db.Monitors.Add(m);
            await db.SaveChangesAsync();
            return Results.Created($"/monitors/{m.Id}", m);
        });

        app.MapGet("/monitors/{id:guid}", async (AppDbContext db, Guid id) =>
            await db.Monitors.FindAsync(id) is { } m ? Results.Ok(m) : Results.NotFound());
    }
}