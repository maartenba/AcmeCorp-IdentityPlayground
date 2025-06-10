using System.Text;
using System.Text.Json;
using AcmeCorp.IdentityServer;
using AcmeCorp.IdentityServer.DynamicProviders;
using Duende.IdentityServer;
using IdentityServerHost.Pages.Portal;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

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
    .AddTestUsers(TestUsers.Users)
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
    .AddCheck<DuendeIdentityServerLicenseHealthCheck>("identityserver");

var app = builder.Build();

app.UseIdentityServer();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapHealthChecks("health", new HealthCheckOptions
{
   ResponseWriter = WriteResponse
});
app.MapRazorPages().RequireAuthorization();

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