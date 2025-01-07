using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcmeCorp.WeatherApp.Pages;

[AllowAnonymous]
public class LogoutSuccessModel : PageModel
{
    private readonly ILogger<LogoutSuccessModel> _logger;

    public LogoutSuccessModel(ILogger<LogoutSuccessModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}