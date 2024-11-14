using Duende.IdentityModel.Client;

namespace AcmeCorp.WeatherApp.Services;

public interface ITokenService
{
    Task<TokenResponse> GetTokenAsync(string scope);
}