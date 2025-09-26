using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

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

var app = builder.Build();

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

// ---------- In-memory store ----------
var store = new ConcurrentDictionary<int, Monitor>();
var nextId = 0;
var m = new Monitor
{
    Id = Interlocked.Increment(ref nextId),
    Name = "Google",
    Url = "https://www.google.com",
    Status = "UP",
    LastCheckedUtc = DateTime.Now
};
store[m.Id] = m;

// ---------- Endpoints ----------
// List
app.MapGet("/monitors", () =>
{
    var items = store.Values.OrderBy(m => m.Id).ToList();
    return Results.Ok(items);
});


// ---------- Swagger ----------
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
// app.UseSwagger();
// app.UseSwaggerUI(o =>
// {
//     o.SwaggerEndpoint("/swagger/v1/swagger.json", "IsItLive API v1");
//     o.RoutePrefix = "swagger";
// });
//
// // Handy redirect from "/" to Swagger
// app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();


app.Run();

public class Monitor
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime? LastCheckedUtc { get; set; } = default!;
}

public record CreateMonitorRequest(string Name, string Url, int? CheckIntervalSeconds);
public record UpdateMonitorRequest(string? Name, string? Url, bool? Enabled, int? CheckIntervalSeconds);

public record CheckResultDto(
    int MonitorId,
    DateTimeOffset CheckedAt,
    int StatusCode,
    bool IsSuccess,
    long ResponseTimeMs,
    string? Error
);

// ---------- App setup ----------