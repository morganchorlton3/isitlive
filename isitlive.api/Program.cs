var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI (always-on while you're learning)
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "IsItLive API v1");
    o.RoutePrefix = "swagger";
});

// Handy redirect from "/" to Swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// A basic health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "ok", ts = DateTimeOffset.UtcNow }));

app.Run();