using Duende.IdentityServer.Models;

namespace AcmeCorp.IdentityServer.DynamicProviders;

public class GoogleIdentityProvider : IdentityProvider
{
    public const string ProviderType = "google";
    
    public GoogleIdentityProvider() 
        : base(ProviderType)
    {
    }
    
    public string? ClientId 
    {
        get => this["ClientId"];
        set => this["ClientId"] = value;
    }
    
    public string? ClientSecret 
    {
        get => this["ClientSecret"];
        set => this["ClientSecret"] = value;
    }
}