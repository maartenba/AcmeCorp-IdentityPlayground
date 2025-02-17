using AcmeCorp.WeatherApp.Models;
using AcmeCorp.WeatherApp.Services;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// registers and configures IOptions<IdentityServerSettings> to section in appSettings.json
builder.Services.Configure<IdentityServerSettings>(builder.Configuration.GetSection("IdentityServerSettings"));

builder.Services.AddSingleton<IDiscoveryCache>(services =>
{
    var identityServerSettings = services.GetRequiredService<IOptions<IdentityServerSettings>>();
    var factory = services.GetRequiredService<IHttpClientFactory>();
    return new DiscoveryCache(identityServerSettings.Value.DiscoveryUrl, () => factory.CreateClient());
});

// Server-side sessions/logout
builder.Services.AddTransient<CookieEventHandler>();
builder.Services.AddSingleton<LogoutSessionManager>();

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<WeatherApiClient>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.EventsType = typeof(CookieEventHandler);
        options.Cookie.Name = "AcmeCorp.WeatherApp";
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["InteractiveServiceSettings:AuthorityUrl"];
        options.ClientId = builder.Configuration["InteractiveServiceSettings:ClientId"];
        options.ClientSecret = builder.Configuration["InteractiveServiceSettings:ClientSecret"];

        options.RequireHttpsMetadata = false;

        options.Scope.Add(builder.Configuration["InteractiveServiceSettings:Scopes:0"]);
        
        options.ResponseType = "code";
        options.ResponseMode = "query";
        options.UsePkce = true;

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.DisableTelemetry = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();