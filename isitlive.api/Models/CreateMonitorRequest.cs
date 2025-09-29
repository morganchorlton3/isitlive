namespace IsItLive.Api.Models;

public sealed class CreateMonitorRequest
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
}
