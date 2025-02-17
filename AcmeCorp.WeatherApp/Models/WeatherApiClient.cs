using AcmeCorp.WeatherApp.Services;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;

namespace AcmeCorp.WeatherApp.Models;


public class WeatherApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ITokenService _tokenService;

    public WeatherApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ITokenService tokenService)
    {
        _httpClientFactory = httpClientFactory;
        _contextAccessor = contextAccessor;
        _tokenService = tokenService;
    }
    
    public async Task<WeatherForecast[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherForecast>? forecasts = null;
        
        var httpContext = _contextAccessor.HttpContext;
        var token = httpContext is null 
            ? (await _tokenService.GetTokenAsync("weatherapi.read")).AccessToken
            : await httpContext.GetTokenAsync("access_token");

        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.SetBearerToken(token ?? throw new InvalidOperationException("Access token is not available."));
        
        await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecast>("http://localhost:5445/weatherforecast", cancellationToken))
        {
            if (forecasts?.Count >= maxItems)
            {
                break;
            }
            if (forecast is not null)
            {
                forecasts ??= [];
                forecasts.Add(forecast);
            }
        }

        return forecasts?.ToArray() ?? [];
    }
}