using Duende.IdentityServer;
using Duende.IdentityServer.Licensing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AcmeCorp.IdentityServer;

public class DuendeIdentityServerLicenseHealthCheck(
    IHostEnvironment environment,
    LicenseUsageSummary? licenseUsageSummary,
    IdentityServerLicense? license = null)
    : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthCheckData = new Dictionary<string, object>();

        healthCheckData["mode"] = license == null
            ? "trial"
            : license.Expiration < DateTime.UtcNow
                ? "expired"
                : "active";

        if (license != null)
        {
            if (license.Expiration != null)
            {
                healthCheckData["expiration"] = license.Expiration;
            }
        }

        if (licenseUsageSummary != null)
        {
            healthCheckData["clients_count"]  = licenseUsageSummary.ClientsUsed.Count;
            healthCheckData["issuers_count"]  = licenseUsageSummary.IssuersUsed.Count;
        }

        if (environment.IsProduction())
        {
            if (license == null)
            {
                return Task.FromResult(
                    new HealthCheckResult(
                        status: context.Registration.FailureStatus,
                        description: "Missing Duende IdentityServer license.",
                        data: healthCheckData));
            }

            if (license != null && license.Expiration < DateTime.Now)
            {
                return Task.FromResult(
                    new HealthCheckResult(
                        status: context.Registration.FailureStatus,
                        description: "Duende IdentityServer license has expired.",
                        data: healthCheckData));
            }
        }

        if (!environment.IsProduction())
        {
            return Task.FromResult(
                HealthCheckResult.Healthy(
                    description: "Duende IdentityServer license is not required in non-production environments.",
                    data: healthCheckData));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                description: "Duende IdentityServer license is valid.",
                data: healthCheckData));
    }
}