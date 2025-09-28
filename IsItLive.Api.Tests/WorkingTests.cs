using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using IsItLive.Api.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using MonitorEntity = IsItLive.Api.Models.Monitor;

namespace IsItLive.Api.Tests;

/// <summary>
/// Working integration tests that use a custom WebApplicationFactory
/// </summary>
public class WorkingTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public WorkingTests()
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
        var monitor = new MonitorEntity
        {
            Name = "Test Monitor",
            Url = "https://example.com",
            Status = "UNKNOWN"
        };

        var json = JsonSerializer.Serialize(monitor);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/monitors", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdMonitor = JsonSerializer.Deserialize<MonitorEntity>(responseContent);
        
        createdMonitor.Should().NotBeNull();
        createdMonitor!.Name.Should().Be("Test Monitor");
        createdMonitor.Url.Should().Be("https://example.com");
        createdMonitor.Status.Should().Be("UNKNOWN");
        createdMonitor.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task PostMonitor_ShouldAssignNewGuid()
    {
        // Arrange
        var monitor = new MonitorEntity
        {
            Name = "Test Monitor",
            Url = "https://example.com",
            Status = "UNKNOWN"
        };

        var json = JsonSerializer.Serialize(monitor);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/monitors", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdMonitor = JsonSerializer.Deserialize<MonitorEntity>(responseContent);
        
        createdMonitor.Should().NotBeNull();
        createdMonitor!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetMonitorById_ShouldReturnMonitor_WhenMonitorExists()
    {
        // Arrange - Create a monitor first
        var monitor = new MonitorEntity
        {
            Name = "Test Monitor",
            Url = "https://example.com",
            Status = "UNKNOWN"
        };

        var json = JsonSerializer.Serialize(monitor);
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
        retrievedMonitor.Name.Should().Be("Test Monitor");
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
    public async Task PostMonitor_ShouldReturnCreatedAtLocation()
    {
        // Arrange
        var monitor = new MonitorEntity
        {
            Name = "Test Monitor",
            Url = "https://example.com",
            Status = "UNKNOWN"
        };

        var json = JsonSerializer.Serialize(monitor);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/monitors", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().MatchRegex("/monitors/[0-9a-f-]+");
    }

    [Fact]
    public async Task GetMonitors_ShouldReturnAllMonitors_OrderedByName()
    {
        // Arrange - Create multiple monitors
        var monitors = new List<MonitorEntity>
        {
            new() { Name = "Zebra Monitor", Url = "https://zebra.com", Status = "UNKNOWN" },
            new() { Name = "Apple Monitor", Url = "https://apple.com", Status = "UNKNOWN" },
            new() { Name = "Banana Monitor", Url = "https://banana.com", Status = "UNKNOWN" }
        };

        foreach (var monitor in monitors)
        {
            var json = JsonSerializer.Serialize(monitor);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _client.PostAsync("/monitors", content);
        }

        // Act
        var response = await _client.GetAsync("/monitors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var retrievedMonitors = JsonSerializer.Deserialize<List<MonitorEntity>>(responseContent);
        
        retrievedMonitors.Should().NotBeNull();
        retrievedMonitors.Should().HaveCount(3);
        retrievedMonitors!.Select(m => m.Name).Should().BeInAscendingOrder();
    }
}
