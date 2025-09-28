using FluentAssertions;
using IsItLive.Api.Models;
using MonitorEntity = IsItLive.Api.Models.Monitor;

namespace IsItLive.Api.Tests;

/// <summary>
/// Simple unit tests that don't require the full web application
/// </summary>
public class SimpleTests
{
    [Fact]
    public void Monitor_ShouldHaveCorrectDefaultValues()
    {
        // Act
        var monitor = new MonitorEntity();

        // Assert
        monitor.Id.Should().Be(Guid.Empty);
        monitor.Name.Should().Be("");
        monitor.Url.Should().Be("");
        monitor.Status.Should().Be("UNKNOWN");
        monitor.LastCheckedUtc.Should().BeNull();
    }

    [Fact]
    public void Monitor_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Monitor";
        var url = "https://example.com";
        var status = "UP";
        var lastChecked = DateTime.UtcNow;

        // Act
        var monitor = new MonitorEntity
        {
            Id = id,
            Name = name,
            Url = url,
            Status = status,
            LastCheckedUtc = lastChecked
        };

        // Assert
        monitor.Id.Should().Be(id);
        monitor.Name.Should().Be(name);
        monitor.Url.Should().Be(url);
        monitor.Status.Should().Be(status);
        monitor.LastCheckedUtc.Should().Be(lastChecked);
    }

    [Theory]
    [InlineData("UP")]
    [InlineData("DOWN")]
    [InlineData("UNKNOWN")]
    [InlineData("MAINTENANCE")]
    public void Monitor_ShouldAcceptValidStatuses(string status)
    {
        // Act
        var monitor = new MonitorEntity { Status = status };

        // Assert
        monitor.Status.Should().Be(status);
    }

    [Fact]
    public void Monitor_ShouldHandleNullLastCheckedUtc()
    {
        // Act
        var monitor = new MonitorEntity { LastCheckedUtc = null };

        // Assert
        monitor.LastCheckedUtc.Should().BeNull();
    }

    [Fact]
    public void Monitor_ShouldHaveEmptyGuidByDefault()
    {
        // Act
        var monitor = new MonitorEntity();

        // Assert
        monitor.Id.Should().Be(Guid.Empty);
    }
}
