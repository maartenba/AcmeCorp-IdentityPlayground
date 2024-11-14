namespace AcmeCorp.WeatherApp.Services;

public class IdentityServerSettings
{
    public string DiscoveryUrl { get; init; }
    public string ClientName { get; init; }
    public string ClientPassword { get; init; }
    public bool UseHttps { get; init; }
}