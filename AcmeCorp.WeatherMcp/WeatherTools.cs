using System.ComponentModel;
using ModelContextProtocol.Server;

namespace AcmeCorp.WeatherMcp;

[McpServerToolType]
public sealed class WeatherTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool, Description("Get weather forecasts.")]
    public async Task<List<WeatherForecast>> GetForecasts()
    {
        List<WeatherForecast> forecasts = new();
        
        var httpClient = _httpClientFactory.CreateClient("weather");
        await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecast>("https://localhost:5445/weatherforecast"))
        {
            if (forecast is not null)
            {
                forecasts ??= [];
                forecasts.Add(forecast);
            }
        }

        return forecasts;
    }
}