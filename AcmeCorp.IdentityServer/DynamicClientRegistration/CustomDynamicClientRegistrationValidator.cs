using Duende.IdentityServer.Configuration.Models;
using Duende.IdentityServer.Configuration.Models.DynamicClientRegistration;
using Duende.IdentityServer.Configuration.Validation.DynamicClientRegistration;

namespace AcmeCorp.IdentityServer.DynamicClientRegistration;

public class CustomDynamicClientRegistrationValidator(ILogger<DynamicClientRegistrationValidator> logger)
    : DynamicClientRegistrationValidator(logger)
{
    protected override Task<IStepResult> SetPublicClientProperties(DynamicClientRegistrationContext context)
    {
        context.Client.AllowedCorsOrigins = context.Request.AllowedCorsOrigins ?? new();
        if (context.Request.RequireClientSecret.HasValue)
        {
            context.Client.RequireClientSecret = context.Request.RequireClientSecret.Value;
        }

        if (context.Request.TokenEndpointAuthenticationMethod == "none")
        {
            context.Client.RequireClientSecret = false;
        }
        return StepResult.Success();
    }
}