using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using IsItLive.Api.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using MonitorEntity = IsItLive.Api.Models.Monitor;

namespace IsItLive.Api.Tests;

public class MonitorsEndpointTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public MonitorsEndpointTests()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task GetMonitors_ShouldReturnEmptyList_WhenNoMonitorsExist()
    {
        // Act
        var response = await _client.GetAsync("/monitors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var monitors = JsonSerializer.Deserialize<List<MonitorEntity>>(content);
        
        monitors.Should().NotBeNull();
        monitors.Should().BeEmpty();
    }

    [Fact]
    public async Task PostMonitor_ShouldCreateMonitor_WhenValidDataProvided()
    {
        // Arrange
        var newMonitor = new
        {
            Name = "Test Monitor",
            Url = "https://example.com",
            Status = "UNKNOWN"
        };

        var json = JsonSerializer.Serialize(newMonitor);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/monitors", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdMonitor = JsonSerializer.Deserialize<MonitorEntity>(responseContent);
        
        createdMonitor.Should().NotBeNull();
        createdMonitor!.Name.Should().Be(newMonitor.Name);
        createdMonitor.Url.Should().Be(newMonitor.Url);
        createdMonitor.Status.Should().Be(newMonitor.Status);
        createdMonitor.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task PostMonitor_ShouldAssignNewGuid()
    {
        // Arrange
        var newMonitor = new
        {
            Name = "Test Monitor",
            Url = "https://example.com",
            Status = "UNKNOWN"
        };

        var json = JsonSerializer.Serialize(newMonitor);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/monitors", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdMonitor = JsonSerializer.Deserialize<MonitorEntity>(responseContent);
        
        createdMonitor!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetMonitorById_ShouldReturnMonitor_WhenMonitorExists()
    {
        // Arrange - Create monitor via API
        var newMonitor = new
        {
            Name = "Test Monitor",
            Url = "https://example.com",
            Status = "UNKNOWN"
        };

        var json = JsonSerializer.Serialize(newMonitor);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/monitors", content);
        var createdMonitor = JsonSerializer.Deserialize<MonitorEntity>(await createResponse.Content.ReadAsStringAsync());

        // Act
        var response = await _client.GetAsync($"/monitors/{createdMonitor!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var retrievedMonitor = JsonSerializer.Deserialize<MonitorEntity>(responseContent);
        
        retrievedMonitor.Should().NotBeNull();
        retrievedMonitor!.Id.Should().Be(createdMonitor.Id);
        retrievedMonitor.Name.Should().Be(createdMonitor.Name);
        retrievedMonitor.Url.Should().Be(createdMonitor.Url);
        retrievedMonitor.Status.Should().Be(createdMonitor.Status);
    }

    [Fact]
    public async Task GetMonitorById_ShouldReturnNotFound_WhenMonitorDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/monitors/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMonitors_ShouldReturnAllMonitors_OrderedByName()
    {
        // Arrange - Create monitors via API
        var monitors = new[]
        {
            new { Name = "Z Monitor", Url = "https://z.com", Status = "UNKNOWN" },
            new { Name = "A Monitor", Url = "https://a.com", Status = "UNKNOWN" },
            new { Name = "M Monitor", Url = "https://m.com", Status = "UNKNOWN" }
        };

        foreach (var monitor in monitors)
        {
            var json = JsonSerializer.Serialize(monitor);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            await _client.PostAsync("/monitors", requestContent);
        }

        // Act
        var response = await _client.GetAsync("/monitors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var retrievedMonitors = JsonSerializer.Deserialize<List<MonitorEntity>>(content);
        
        retrievedMonitors.Should().NotBeNull();
        retrievedMonitors.Should().HaveCount(3);
        retrievedMonitors!.Select(m => m.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task PostMonitor_ShouldReturnCreatedAtLocation()
    {
        // Arrange
        var newMonitor = new
        {
            Name = "Test Monitor",
            Url = "https://example.com",
            Status = "UNKNOWN"
        };

        var json = JsonSerializer.Serialize(newMonitor);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/monitors", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("/monitors/");
    }
}
