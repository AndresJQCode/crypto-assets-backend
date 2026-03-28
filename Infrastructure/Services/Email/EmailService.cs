using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Email;

/// <summary>
/// Main email service implementation with business logic
/// Uses IEmailProvider strategy for actual email delivery
/// </summary>
public class EmailService(
    IEmailProvider emailProvider,
    IEmailTemplateService emailTemplateService,
    IOptionsMonitor<AppSettings> appSettings,
    ILogger<EmailService> logger) : IEmailService
{
    private string CompanyName => appSettings.CurrentValue.CompanyName;
    private string FromEmail => appSettings.CurrentValue.EmailService.FromEmail;
    private string HeaderImage => appSettings.CurrentValue.EmailService.HeaderImage;

    public async Task SendConfirmationLinkAsync(
        string email,
        string fullName,
        string confirmationLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = $"Confirma tu email - {CompanyName}";
            var content = await emailTemplateService.RenderConfirmationEmailAsync(
                fullName,
                confirmationLink,
                HeaderImage);

            await emailProvider.SendEmailAsync(
                from: FromEmail,
                to: new[] { email },
                subject: subject,
                htmlBody: content,
                cancellationToken: cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Confirmation email sent successfully to {Email} using {Provider}",
                    email,
                    emailProvider.ProviderName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending confirmation email to {Email}", email);
            throw;
        }
    }

    public async Task SendPasswordResetLinkAsync(
        string email,
        string fullName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = $"Restablecer contraseña - {CompanyName}";
            var content = await emailTemplateService.RenderPasswordResetEmailAsync(
                fullName,
                resetLink,
                HeaderImage);

            await emailProvider.SendEmailAsync(
                from: FromEmail,
                to: new[] { email },
                subject: subject,
                htmlBody: content,
                cancellationToken: cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Password reset email sent successfully to {Email} using {Provider}",
                    email,
                    emailProvider.ProviderName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending password reset email to {Email}", email);
            throw;
        }
    }

    public async Task SendPasswordResetCodeAsync(
        string email,
        string fullName,
        string resetCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = $"Código de restablecimiento - {CompanyName}";
            var content = await emailTemplateService.RenderPasswordResetCodeEmailAsync(
                fullName,
                resetCode,
                HeaderImage);

            await emailProvider.SendEmailAsync(
                from: FromEmail,
                to: new[] { email },
                subject: subject,
                htmlBody: content,
                cancellationToken: cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Password reset code email sent successfully to {Email} using {Provider}",
                    email,
                    emailProvider.ProviderName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending password reset code email to {Email}", email);
            throw;
        }
    }

    public async Task SendCustomEmailAsync(
        string from,
        IReadOnlyCollection<string> to,
        string subject,
        string htmlBody,
        IReadOnlyCollection<string>? cc = null,
        IReadOnlyCollection<string>? bcc = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await emailProvider.SendEmailAsync(
                from: from,
                to: to,
                subject: subject,
                htmlBody: htmlBody,
                cc: cc,
                bcc: bcc,
                cancellationToken: cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Custom email sent successfully to {Recipients} using {Provider}",
                    string.Join(", ", to),
                    emailProvider.ProviderName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error sending custom email to {Recipients} with subject '{Subject}'",
                string.Join(", ", to),
                subject);
            throw;
        }
    }
}
