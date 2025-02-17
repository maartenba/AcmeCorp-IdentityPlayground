using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using IdentityModel;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;

namespace AcmeCorp.Console;

public static class Program
{
    static string _api = "http://localhost:5445/weatherforecast";

    static OidcClient _oidcClient;
    static HttpClient _apiClient = new HttpClient { BaseAddress = new Uri(_api) };

    public static async Task Main()
    {
        System.Console.WriteLine("Press any key to sign in...");
        System.Console.ReadKey();

        await SignIn();
    }

    // lang=json
    private static string rsaKey = """
                                   {
                                       "p": "6aGRqjEmep-ib8XvwYPVz9C9sEUKVQ07FXaBDWuVHNfVFi9yafZVEXUBJHJNb16Bx35yTp22SUjEsGkVlMoUCCoVTV_p-QieErTZ8tkHy8BLwjsxulwiNdUeNLsvMr4KYwc6A1v_OWLoqGJuBCSILBtITMc_vm5gOuVYeblsO9U",
                                       "kty": "RSA",
                                       "q": "lSk8O5TOLcnU1oxcFmEsZvrh-bOg1xyYIcdmY6XCLgLovA7sp3NNxini1tkGPUIljZ1t6Z8asmehZlAoPqL9cOPIs_brYheLwkN66ZzdFQqkjri0OHslexERkZcVDhOYX-gk9VTsANr4FVtd3XN4e2z9oGfTvhs_5y0jJoegcz8",
                                       "d": "gI3x5wEPTV2RWrOEJzx2rwfSsfdrSbaMfMRxhUavs9eVpX-6VbUkUtBRpv3tN8g0OaauZCamUTJCeeoLDjerbGJPMnw2R08ugX9dqJhkapBX_D0tolHwfLgzypBL0IYkKlzjeV8zrVcr1NCIixqr7J0fW0l8HtrhmNhlBQd13am2biCY9L2qOYUxkyLWTx5B9KY53tfQU7Yx7KiFjq8uc1xZ4DuMZpcrExJgnrr5-OeaFxxIX4F5r-aq3z0FM-E-LEqk8yGm1Tn1QSGvIMfmu7HzZAdLQLsAxS-O3v9QgKKaBCEeoBdfzY2LPjHTTJDSprm1pdPKwSN5moxutkIGCQ",
                                       "e": "AQAB",
                                       "use": "sig",
                                       "kid": "gh2BICAt02U7phWeOSlLH3oSJNCj8rwdjQyOm260Cs0",
                                       "qi": "atHh5yJtX01Wrjrm4Zo_Qd80oGlgqeBk7VxOjIKSa8MTFfpLoucNOnB8JCCCK8M9YMJAkH0SBYq999G3Ig5K5PKkxs0TkxIcHLTWKe9BPez1Xe0t2IVDNFLuSoqNm-eY9rl340oZQffcwbg74gbO2YfO2dzatt0K01svzYCM0jA",
                                       "dp": "zQmK-T5hcsSenlGsdLzq7JNZplUxGCugatmxsMF1__Y7gFjrpKsXRVbWRdI6uz7JzvbeArjOvcorNFdFJWuj4PZt85X1lSVG9Uva6xIlkV-WHUQuBEvPikcbV1PfvDykiPxSoZgfiZGQdhbMTr5w86SN8zP6cXoU4htdZpagsnE",
                                       "alg": "RS256",
                                       "dq": "f08ymafoUixx_KzP7EoEG-EF_pnSLce6ERqfyW0wzxSsj9YtJr7DIt-dPML_EEnkw3oa1ct2fc71ct33e36h0jiYlQGq5Y4zOxtdTQcVq1_qE-aR66Mv2lo3JIxO7DTNWQ5KfRp-VDyQgan584kazD26O65ii1-2qxlbZwdbawE",
                                       "n": "iCCroKELTEoZyW3gTW8lc7-QQtF-ERtpgHtJsgPVkm0ljLoVV0tmm-7cb_WxdWo1ObFaPKaan2hDMNslOUMJHMzLgmtbVwvZiQGbBB_FYEA0u2VcD0T8BxnKvn5j9hj2GHumZoRXXGmlY9skk2olXUpXd82iR_FXwkLw_sgid-YlfA60nE0x_6YJN5s8xlLha_SzQwR0kUJ37LsfP5Rj6ydNWLrJ_uv7r6Pr4PGOSpmnV3CvrTYlQq4-4wBN595itknaImZlM3_weQL77nNt_wew2lSXfaKOJp6DFoj2olIvUDkdb0vgPmNaN3lvL2mX5jwkPHaIXf0Lu2wQ-KNoaw"
                                   }
                                   """;

    private static string CreateClientToken(SigningCredentials credential, string clientId, string audience)
    {
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            clientId,
            audience,
            new List<Claim>()
            {
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()),
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
            },
            now,
            now.AddMinutes(1),
            credential
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private static async Task SignIn()
    {
        // create a redirect URI using an available port on the loopback address.
        // requires the OP to allow random ports on 127.0.0.1 - otherwise set a static port
        var browser = new SystemBrowser();
        var redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");
        var authority = "http://localhost:5443";

        var jwk = new JsonWebKey(rsaKey);
        var credential = new SigningCredentials(jwk, "RS256");

        var options = new OidcClientOptions
        {
            Authority = authority,
            ClientId = "interactive.confidential.short.jwt",
            GetClientAssertionAsync = () => Task.FromResult(new ClientAssertion
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = CreateClientToken(credential, "interactive.confidential.short.jwt", authority)
            }),
            RedirectUri = redirectUri,
            Scope = "openid profile weatherapi.read offline_access",
            FilterClaims = false,

            Browser = browser,
            IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
            RefreshTokenInnerHttpHandler = new SocketsHttpHandler(),
        };

        _oidcClient = new OidcClient(options);
        var result = await _oidcClient.LoginAsync(new LoginRequest());

        _apiClient = new HttpClient(result.RefreshTokenHandler)
        {
            BaseAddress = new Uri(_api)
        };

        ShowResult(result);
        await NextSteps(result);
    }

    private static void ShowResult(LoginResult result)
    {
        if (result.IsError)
        {
            System.Console.WriteLine("\n\nError:\n{0}", result.Error);
            return;
        }

        System.Console.WriteLine("\n\nClaims:");
        foreach (var claim in result.User.Claims)
        {
            System.Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
        }

        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.TokenResponse.Raw);

        System.Console.WriteLine($"token response...");
        foreach (var item in values)
        {
            System.Console.WriteLine($"{item.Key}: {item.Value}");
        }
    }

    private static async Task NextSteps(LoginResult result)
    {
        var menu = "  x...exit  c...call api   ";

        while (true)
        {
            System.Console.WriteLine("\n\n");

            System.Console.Write(menu);
            var key = System.Console.ReadKey();

            if (key.Key == ConsoleKey.X) return;
            if (key.Key == ConsoleKey.C) await CallApi();
        }
    }

    private static async Task CallApi()
    {
        var response = await _apiClient.GetAsync("");

        if (response.IsSuccessStatusCode)
        {
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            System.Console.WriteLine("\n\n");
            System.Console.WriteLine(json.RootElement);
        }
        else
        {
            System.Console.WriteLine($"Error: {response.ReasonPhrase}");
        }
    }
}