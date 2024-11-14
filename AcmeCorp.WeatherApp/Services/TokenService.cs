using Duende.IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace AcmeCorp.WeatherApp.Services;

public class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TokenService> _logger;
    private readonly IOptions<IdentityServerSettings> _identityServerSettings;
    private readonly DiscoveryDocumentResponse _discoveryDocument;

    public TokenService(IHttpClientFactory httpClientFactory, ILogger<TokenService> logger, IOptions<IdentityServerSettings> identityServerSettings)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _identityServerSettings = identityServerSettings;
    
        using var httpClient = _httpClientFactory.CreateClient();
        _discoveryDocument = httpClient.GetDiscoveryDocumentAsync(identityServerSettings.Value.DiscoveryUrl).Result;
        if (_discoveryDocument.IsError)
        {
            logger.LogError($"Unable to get discovery document. Error is: {_discoveryDocument.Error}");
            throw new Exception("Unable to get discovery document", _discoveryDocument.Exception);
        }
    }

    public async Task<TokenResponse> GetTokenAsync(string scope)
    {
        using var client = _httpClientFactory.CreateClient();
        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = _discoveryDocument.TokenEndpoint,
      
            ClientId = _identityServerSettings.Value.ClientName,
            ClientSecret = _identityServerSettings.Value.ClientPassword,
            Scope = scope
        });
      
        if (tokenResponse.IsError)
        {
            _logger.LogError($"Unable to get token. Error is: {tokenResponse.Error}");
            throw new Exception("Unable to get token", tokenResponse.Exception);
        }
      
        return tokenResponse;
    }

}