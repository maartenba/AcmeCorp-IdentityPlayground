using Duende.IdentityServer.Models;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
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
            UserClaims = { "role" }
        }
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
                BackChannelLogoutUri = "https://localhost:5444/logout",
                PostLogoutRedirectUris = { "https://localhost:5444/signout-callback-oidc" },
                CoordinateLifetimeWithUserSession = true, // slide server-side session

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "weatherapi.read" },
                
                RequireConsent = true
            },
        };
}