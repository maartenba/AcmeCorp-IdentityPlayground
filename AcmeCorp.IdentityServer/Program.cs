using System.Reflection;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

const string connectionString = @"Server=localhost,1433;Database=identityserver;User Id=sa;Password=FYL0g0Yn58xxUJT2al3tTq5qku0NmAxFRfAUHF7M;TrustServerCertificate=True";
var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

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
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = builder =>
            builder.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.EnableTokenCleanup = true;
        options.TokenCleanupInterval = 30;

        options.ConfigureDbContext = builder =>
            builder.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
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

var app = builder.Build();

app.UseIdentityServer();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

InitializeDatabase(app);

app.Run();

static void InitializeDatabase(IApplicationBuilder app)
{
    using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope())
    {
        var persistedGrantsContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        persistedGrantsContext.Database.Migrate();

        var configurationContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        configurationContext.Database.Migrate();
    }
}