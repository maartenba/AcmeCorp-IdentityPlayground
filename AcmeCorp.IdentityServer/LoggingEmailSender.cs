using Microsoft.AspNetCore.Identity.UI.Services;

namespace AcmeCorp.IdentityServer;

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.BeginScope("Email sent: " + subject );
        logger.LogInformation("To: " + email);
        logger.LogInformation("Subject: " + subject);
        logger.LogInformation(htmlMessage);
        return Task.CompletedTask;
    }
}