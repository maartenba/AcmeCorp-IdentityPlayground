using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Security.Claims;
using AcmeCorp.WeatherMcp;
using Duende.AccessTokenManagement;
using Microsoft.IdentityModel.JsonWebTokens;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

var serverUrl = "http://localhost:5200/";
var identityServerUrl = "https://localhost:5443";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Configure to validate tokens from our in-memory OAuth server
        options.Audience = identityServerUrl;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            
            // -- signing key validation disabled for demo purposes --
            ValidateIssuerSigningKey = false,
            SignatureValidator = (token, _) =>
            {
                var jwt = new JsonWebToken(token);
                return jwt;
            },
            // -- / --
            
            ValidAudience = serverUrl, // Validate that the audience matches the resource metadata as suggested in RFC 8707
            ValidIssuer = identityServerUrl,
            NameClaimType = "name",
            RoleClaimType = "roles"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var name = context.Principal?.Identity?.Name ?? "unknown";
                var email = context.Principal?.FindFirstValue("preferred_username") ?? "unknown";
                Console.WriteLine($"Token validated for: {name} ({email})");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Challenging client to authenticate");
                return Task.CompletedTask;
            }
        };
    })
    .AddMcp(options =>
    {
        options.ResourceMetadata = new()
        {
            Resource = new Uri(serverUrl),
            AuthorizationServers = { new Uri(identityServerUrl) },
            
            ScopesSupported = ["mcp:tools", "mcp:read", "mcp:prompts", "weatherapi.read"],
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "Weather MCP Server",
            Title = "Weather MCP Server",
            Version = "1.0.0"
        };
    })
    .WithTools<WeatherTools>()
    .WithHttpTransport();

// Configure HttpClientFactory for weather API
builder.Services.AddClientCredentialsTokenManagement()
    .AddClient(ClientCredentialsClientName.Parse("weather.client"), client =>
    {
        client.TokenEndpoint = new Uri("https://localhost:5443/connect/token");

        client.ClientId = ClientId.Parse("m2m.client");
        client.ClientSecret = ClientSecret.Parse("511536EF-F270-4058-80CA-1C89C192F69A");

        client.Scope = Scope.Parse("weatherapi.read");
    });
builder.Services.AddClientCredentialsHttpClient("weather", ClientCredentialsClientName.Parse("weather.client"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Use the default MCP policy name that we've configured
app.MapMcp().RequireAuthorization();

Console.WriteLine($"Starting MCP server with authorization at {serverUrl}");
Console.WriteLine($"Using in-memory OAuth server at {identityServerUrl}");
Console.WriteLine($"Protected Resource Metadata URL: {serverUrl}.well-known/oauth-protected-resource");
Console.WriteLine("Press Ctrl+C to stop the server");

app.Run(serverUrl);