using System.Net.Http.Json;

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
}