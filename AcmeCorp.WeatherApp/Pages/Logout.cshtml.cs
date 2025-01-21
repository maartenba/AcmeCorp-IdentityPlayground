using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using AcmeCorp.WeatherApp.Services;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace AcmeCorp.WeatherApp.Pages;

[IgnoreAntiforgeryToken]
public class LogoutModel : PageModel
{
    private readonly LogoutSessionManager _logoutSessions;
    private readonly IDiscoveryCache _discoveryCache;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(LogoutSessionManager logoutSessions, IDiscoveryCache discoveryCache, ILogger<LogoutModel> logger)
    {
        _logoutSessions = logoutSessions;
        _discoveryCache = discoveryCache;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostBackChannelAsync([FromForm(Name = "logout_token")] string? logoutToken)
    {
        Response.Headers.Append("Cache-Control", "no-cache, no-store");
        Response.Headers.Append("Pragma", "no-cache");

        if (string.IsNullOrEmpty(logoutToken))
        {
            // Interactive logout initiated by user
            _logger.LogInformation("Interactive logout triggered");
            return SignOut(OpenIdConnectDefaults.AuthenticationScheme);
        }

        // Backchannel logout triggered by IdentityServer
        try
        {
            var user = await ValidateLogoutToken(logoutToken);

            // these are the sub & sid to signout
            var sub = user.FindFirst("sub")?.Value;
            var sid = user.FindFirst("sid")?.Value;

            _logoutSessions.Add(sub, sid);

            return StatusCode(200);
        }
        catch { }

        return BadRequest();
    }

    private async Task<ClaimsPrincipal> ValidateLogoutToken(string logoutToken)
    {
        var claims = await ValidateJwt(logoutToken);

        if (claims.FindFirst("sub") == null && claims.FindFirst("sid") == null) throw new Exception("Invalid logout token");

        var nonce = claims.FindFirstValue("nonce");
        if (!String.IsNullOrWhiteSpace(nonce)) throw new Exception("Invalid logout token");

        var eventsJson = claims.FindFirst("events")?.Value;
        if (String.IsNullOrWhiteSpace(eventsJson)) throw new Exception("Invalid logout token");

        var events = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(eventsJson);
        var logoutEvent = events.TryGetValue("http://schemas.openid.net/event/backchannel-logout", out _);
        if (logoutEvent == false) throw new Exception("Invalid logout token");

        return claims;
    }

    private async Task<ClaimsPrincipal> ValidateJwt(string jwt)
    {
        // read discovery document to find issuer and key material
        var disco = await _discoveryCache.GetAsync();

        var keys = new List<SecurityKey>();
        foreach (var webKey in disco.KeySet.Keys)
        {
            var key = new JsonWebKey
            {
                Kty = webKey.Kty,
                Alg = webKey.Alg,
                Kid = webKey.Kid,
                X = webKey.X,
                Y = webKey.Y,
                Crv = webKey.Crv,
                E = webKey.E,
                N = webKey.N,
            };
            keys.Add(key);
        }

        var parameters = new TokenValidationParameters
        {
            ValidIssuer = disco.Issuer,
            ValidAudience = "interactive",
            IssuerSigningKeys = keys,

            NameClaimType = JwtClaimTypes.Name,
            RoleClaimType = JwtClaimTypes.Role
        };

        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();

        var user = handler.ValidateToken(jwt, parameters, out var _);
        return user;
    }
}