using Duende.IdentityServer;
using Duende.IdentityServer.Models;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new IdentityResource
        {
            Name = "role",
            UserClaims = { "role" }
        }
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new("weatherapi.read", "Weather API read access"),
        new("weatherapi.write", "Weather API write access")
    ];

    public static IEnumerable<ApiResource> ApiResources => new[]
    {
        new ApiResource("weatherapi")
        {
            Scopes = { "weatherapi.read", "weatherapi.write" },
            ApiSecrets = { new Secret("ScopeSecret".Sha256()) },
            UserClaims = { "email", "role" }
        }
    };

    private static Secret PublicKey = new Secret
    {
        Type = IdentityServerConstants.SecretTypes.JsonWebKey,
        Value =
            """
            {
                "kty": "RSA",
                "e": "AQAB",
                "use": "sig",
                "kid": "gh2BICAt02U7phWeOSlLH3oSJNCj8rwdjQyOm260Cs0",
                "alg": "RS256",
                "n": "iCCroKELTEoZyW3gTW8lc7-QQtF-ERtpgHtJsgPVkm0ljLoVV0tmm-7cb_WxdWo1ObFaPKaan2hDMNslOUMJHMzLgmtbVwvZiQGbBB_FYEA0u2VcD0T8BxnKvn5j9hj2GHumZoRXXGmlY9skk2olXUpXd82iR_FXwkLw_sgid-YlfA60nE0x_6YJN5s8xlLha_SzQwR0kUJ37LsfP5Rj6ydNWLrJ_uv7r6Pr4PGOSpmnV3CvrTYlQq4-4wBN595itknaImZlM3_weQL77nNt_wew2lSXfaKOJp6DFoj2olIvUDkdb0vgPmNaN3lvL2mX5jwkPHaIXf0Lu2wQ-KNoaw"
            }
            """
    };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "weatherapi.read", "weatherapi.write" }
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5444/signin-oidc" },
                //BackChannelLogoutUri = "https://localhost:5444/logout",
                FrontChannelLogoutUri = "https://localhost:5444/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5444/" },
                CoordinateLifetimeWithUserSession = true, // slide server-side session

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "email", "weatherapi.read" },
                
                RequireConsent = true
            },

            new Client
            {
                ClientId = "interactive.confidential.short.jwt",
                ClientName = "Interactive client (Code with PKCE) using private key JWT authentication with short access token lifetime",

                RedirectUris = { "https://notused" },
                PostLogoutRedirectUris = { "https://notused" },

                ClientSecrets = { PublicKey },

                AllowedGrantTypes = GrantTypes.Code,
                RequireRequestObject = false,
                AllowedScopes = { "openid", "profile", "email", "weatherapi.read" },

                RequireConsent = true,

                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,

                AccessTokenLifetime = 75
            },
        };
}