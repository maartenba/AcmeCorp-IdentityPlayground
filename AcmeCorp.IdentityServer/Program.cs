using Duende.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;

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

        options.ServerSideSessions.UserDisplayNameClaimType = "name"; // this sets the "name" claim as the display name in the admin tool
        options.ServerSideSessions.RemoveExpiredSessions = true; // removes expired sessions. defaults to true.
        options.ServerSideSessions.ExpiredSessionsTriggerBackchannelLogout = true; // this triggers notification to clients. defaults to false.
    })
    .AddTestUsers(TestUsers.Users)
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddServerSideSessions()
    .AddJwtBearerClientAuthentication();

builder.Services.AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.ClientId = "517997330312-a8g0f4plbr5cb54pntcu3j59tdbsu1pi.apps.googleusercontent.com";
        options.ClientSecret = "OezONvJiWNfn9GrRpQt3hQ-_";
        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

        options.ClaimActions.MapAll();
    });

var app = builder.Build();

app.UseIdentityServer();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

app.Run();