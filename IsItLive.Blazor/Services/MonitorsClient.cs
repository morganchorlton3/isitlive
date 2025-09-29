using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public sealed class MonitorsClient
{
    private readonly IHttpClientFactory _factory;
    public MonitorsClient(IHttpClientFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<Monitor>> GetMonitorsAsync(CancellationToken ct = default)
    {
        var http = _factory.CreateClient("IsItLiveApi");
        // If your endpoint is GET /monitors:
        var items = await http.GetFromJsonAsync<List<Monitor>>("monitors", cancellationToken: ct);
        return items ?? [];
    }

    public async Task<Monitor?> CreateMonitorAsync(CreateMonitorRequest request, CancellationToken ct = default)
    {
        var http = _factory.CreateClient("IsItLiveApi");
        var response = await http.PostAsJsonAsync("monitors", request, cancellationToken: ct);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Monitor>(cancellationToken: ct);
        }
        
        return null;
    }
}

public class CreateMonitorRequest
{
    [JsonPropertyName("name"), Required, MinLength(3)]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("url"), Required, Url] 
    public string Url { get; set; } = "";
}