using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace IsItLive.Api.Tests;

public class HealthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ok");
        content.Should().Contain("ts");
    }

    [Fact]
    public async Task GetHealth_ShouldReturnValidTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ok");
        content.Should().Contain("ts");
        
        // Parse the JSON manually to avoid deserialization issues
        var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
        var status = jsonDoc.RootElement.GetProperty("status").GetString();
        var timestamp = jsonDoc.RootElement.GetProperty("ts").GetDateTimeOffset();
        
        status.Should().Be("ok");
        timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    private record HealthResponse(string Status, DateTimeOffset Ts);
}
