namespace IsItLive.Api.Models;

public sealed class Monitor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public string Status { get; set; } = "UNKNOWN";
    public DateTime? LastCheckedUtc { get; set; }
}