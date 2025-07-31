using AcmeCorp.IdentityServer.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcmeCorp.IdentityServer;

public static class PasskeyEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapPasskeyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/.well-known/passkey-endpoints", async (
                HttpContext http,
                CancellationToken cancellationToken) =>
            {
                // https://w3c.github.io/webappsec-passkey-endpoints/
            
                http.Response.Headers.ContentType = "application/json";
                await http.Response.WriteAsync(
                    """
                    {
                        "enroll": "https://localhost:5443/identity/account/manage/passkeys",
                        "manage": "https://localhost:5443/identity/account/manage/passkeys"
                    }
                    """, cancellationToken: cancellationToken);
            })
            .ExcludeFromDescription();
        
        var accountGroup = endpoints.MapGroup("/Identity/Account").ExcludeFromDescription();
        
        accountGroup.MapPost("/PasskeyCreationOptions", async (
            HttpContext context,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] IAntiforgery antiforgery) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            
            PasskeyUserEntity? passkeyUserEntity = null;
            
            // 1. Try to associate new passkey with currently logged-in user
            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                var userId = await userManager.GetUserIdAsync(user);
                var userName = await userManager.GetUserNameAsync(user) ?? "User";
                passkeyUserEntity = new PasskeyUserEntity
                {
                    Id = userId,
                    Name = userName,
                    DisplayName = userName
                };
            }
            
            // 2. Fall back to usernameless passkey
            if (passkeyUserEntity == null)
            {
                var userIdentifier = Guid.NewGuid().ToString();
                passkeyUserEntity = new()
                {
                    Id = userIdentifier,
                    Name = userIdentifier,
                    DisplayName = "Unnamed passkey"
                };
            }
            
            var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(passkeyUserEntity);
            return TypedResults.Content(optionsJson, contentType: "application/json");
        });

        accountGroup.MapPost("/PasskeyRequestOptions", async (
            HttpContext context,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] IAntiforgery antiforgery,
            [FromQuery] string? username) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            
            var user = string.IsNullOrEmpty(username) ? null : await userManager.FindByNameAsync(username);
            var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user);
            return TypedResults.Content(optionsJson, contentType: "application/json");
        });

        return accountGroup;
    }
}