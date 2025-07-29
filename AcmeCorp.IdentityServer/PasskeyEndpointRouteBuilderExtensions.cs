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
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
            {
                return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
            }

            var userId = await userManager.GetUserIdAsync(user);
            var userName = await userManager.GetUserNameAsync(user) ?? "User";
            var userEntity = new PasskeyUserEntity(userId, userName, displayName: userName);
            var passkeyCreationArgs = new PasskeyCreationArgs(userEntity);
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