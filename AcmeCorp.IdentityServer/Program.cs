using System.Text;
using System.Text.Json;
using AcmeCorp.IdentityServer;
using AcmeCorp.IdentityServer.Data;
using AcmeCorp.IdentityServer.DynamicProviders;
using AcmeCorp.IdentityServer.Models;
using Duende.IdentityServer;
using IdentityServerHost.Pages.Portal;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NReco.Logging.File;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSingleton<IEmailSender, LoggingEmailSender>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityPasskeyOptions>(options =>
{
    // Explicitly add https://localhost:5443 to overcome local Kestrel sometimes not providing current origin while debugging
    // Do not add this in production!
    options.ValidateOrigin = context => ValueTask.FromResult(
        !context.CrossOrigin || context.Origin == "https://localhost:5443");
});

builder.Services
    .AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;

        options.EmitStaticAudienceClaim = true;

        options.PushedAuthorization.AllowUnregisteredPushedRedirectUris = true;
        
        options.DynamicProviders.PathPrefix = "/federation";
        options.DynamicProviders
            .AddProviderType<GoogleHandler, GoogleOptions, GoogleIdentityProvider>(
                GoogleIdentityProvider.ProviderType);

        options.ServerSideSessions.UserDisplayNameClaimType = "name"; // this sets the "name" claim as the display name in the admin tool
        options.ServerSideSessions.RemoveExpiredSessions = true; // removes expired sessions. defaults to true.
        options.ServerSideSessions.ExpiredSessionsTriggerBackchannelLogout = true; // this triggers notification to clients. defaults to false.
        
        options.Diagnostics.LogFrequency = TimeSpan.FromMinutes(10);
    })
    .AddInMemoryIdentityProviders([
        new GoogleIdentityProvider
        {
            Scheme = "google1",
            DisplayName = "Google (dynamic)",
            Enabled = true,
            ClientId = "...",
            ClientSecret = "..."
        }
    ])
    .AddAspNetIdentity<ApplicationUser>()
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddServerSideSessions()
    .AddJwtBearerClientAuthentication()
    .AddLicenseSummary();

builder.Services.ConfigureOptions<GoogleDynamicConfigureOptions>();

builder.Services.AddAuthentication()
    .AddGoogleOpenIdConnect(authenticationScheme: "Google", displayName: "Google", configureOptions: options =>
    {
        options.ClientId = "517997330312-a8g0f4plbr5cb54pntcu3j59tdbsu1pi.apps.googleusercontent.com";
        options.ClientSecret = "OezONvJiWNfn9GrRpQt3hQ-_";
        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        options.CallbackPath = "/signin-google";
        
        // options.Events.OnTicketReceived = n =>
        // {
        //     // var idSvc =
        //     //     n.HttpContext.RequestServices.GetRequiredService<MovieIdentityService>();
        //     //
        //     // var appClaims =
        //     //     idSvc.GetClaimsForUser(n.Principal.FindFirst("sub")?.Value);
        //
        //     n.Principal.Identities.First().AddClaim(new Claim("yo", "yo"));
        //
        //     return Task.CompletedTask;
        // };
    });

builder.Services.AddSingleton<ClientRepository>();

builder.Services.AddHealthChecks()
    .AddCheck<DuendeIdentityServerLicenseHealthCheck>("identityserver", HealthStatus.Degraded);

builder.Services.AddLogging(configure =>
{
    configure.AddFile("diagnostics.log", options =>
    {
        options.Append = true;
        options.FilterLogEntry = entry =>
            entry.LogName == "Duende.IdentityServer.Diagnostics.Summary";
    });
});


builder.Services.AddDataProtection()
    .SetApplicationName("AcmeCorp.IdentityServer")
    .PersistKeysToFileSystem(new DirectoryInfo("./keys"));

var app = builder.Build();

app.UseIdentityServer();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapHealthChecks("health", new HealthCheckOptions
{
   ResponseWriter = WriteResponse
});
app.MapPasskeyEndpoints();
app.MapRazorPages().RequireAuthorization();

await SeedData.EnsureSeedDataAsync(app);
app.Run();

static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status",
                healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description",
                healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);

                JsonSerializer.Serialize(jsonWriter, item.Value,
                    item.Value?.GetType() ?? typeof(object));
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));
}