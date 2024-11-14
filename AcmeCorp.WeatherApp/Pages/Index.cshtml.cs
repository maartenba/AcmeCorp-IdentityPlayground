using AcmeCorp.WeatherApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcmeCorp.WeatherApp.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly WeatherApiClient _weatherApiClient;
    private readonly ILogger<IndexModel> _logger;

    public List<WeatherForecast> Forecasts { get; set; } = new(0);

    public IndexModel(WeatherApiClient weatherApiClient, ILogger<IndexModel> logger)
    {
        _weatherApiClient = weatherApiClient;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        Forecasts = new( await _weatherApiClient.GetWeatherAsync());
    }
}