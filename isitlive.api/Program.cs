using IsItLive.Api.Data;
using MonitorEntity = IsItLive.Api.Models.Monitor;
using Microsoft.EntityFrameworkCore;
using Monitor = System.Threading.Monitor;

// ---------- Models / DTOs ----------
var builder = WebApplication.CreateBuilder(args);

// CORS: allow your Blazor app (adjust port if needed)
var blazorOrigin = builder.Configuration["BlazorOrigin"] ?? "http://localhost:5001";
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .WithOrigins(blazorOrigin)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Swagger (nice while learning)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Shared HttpClient
builder.Services.AddHttpClient("monitor");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

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

app.Run();