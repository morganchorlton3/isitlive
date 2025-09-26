using System.Text.Json.Serialization;

public sealed class Monitor
{
    [JsonPropertyName("id")] public string Id { get; set; } = default!;
    [JsonPropertyName("name")] public string Name { get; set; } = default!;
    [JsonPropertyName("url")] public string Url { get; set; } = default!;
    [JsonPropertyName("status")] public string Status { get; set; } = default!; // e.g. "UP"/"DOWN"
    [JsonPropertyName("lastCheckedUtc")] public DateTime? LastCheckedUtc { get; set; }
}