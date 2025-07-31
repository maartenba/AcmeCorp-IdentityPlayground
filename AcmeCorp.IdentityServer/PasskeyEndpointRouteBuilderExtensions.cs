using AcmeCorp.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcmeCorp.IdentityServer;

public static class PasskeyEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapPasskeyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Identity/Account").ExcludeFromDescription();
        
        accountGroup.MapPost("/PasskeyCreationOptions", async (
            HttpContext context,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            PasskeyUserEntity? passkeyUserEntity = null;
            
            // 1. Try to associate new passkey with currently logged-in user
            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                var userId = await userManager.GetUserIdAsync(user);
                var userName = await userManager.GetUserNameAsync(user) ?? "User";
                passkeyUserEntity = new PasskeyUserEntity(userId, userName, displayName: userName);
            }
            
            // 2. Fall back to usernameless passkey
            if (passkeyUserEntity == null)
            {
                var userIdentifier = Guid.NewGuid().ToString();
                passkeyUserEntity = new PasskeyUserEntity(userIdentifier, userIdentifier, displayName: "Unnamed passkey");
            }
            
            var passkeyCreationArgs = new PasskeyCreationArgs(passkeyUserEntity);
            var options = await signInManager.ConfigurePasskeyCreationOptionsAsync(passkeyCreationArgs);
            return TypedResults.Content(options.AsJson(), contentType: "application/json");
        });

        accountGroup.MapPost("/PasskeyRequestOptions", async (
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromQuery] string? username) =>
        {
            var user = string.IsNullOrEmpty(username) ? null : await userManager.FindByNameAsync(username);
            var passkeyRequestArgs = new PasskeyRequestArgs<ApplicationUser>
            {
                User = user,
            };
            var options = await signInManager.ConfigurePasskeyRequestOptionsAsync(passkeyRequestArgs);
            return TypedResults.Content(options.AsJson(), contentType: "application/json");
        });

        return accountGroup;
    }
}