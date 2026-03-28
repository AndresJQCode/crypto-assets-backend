using Domain.Interfaces;
using Infobip.Api.Client;
using Infobip.Api.Client.Api;
using Infobip.Api.Client.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Email;

public class InfobipEmailSender<TUser>(
    IOptionsMonitor<AppSettings> appSettings,
    IEmailTemplateService emailTemplateService,
    ILogger<InfobipEmailSender<TUser>> logger) : IEmailSender<TUser>, IDisposable where TUser : class
{
    private readonly EmailApi _infobipClient = new(new Configuration()
    {
        BasePath = appSettings.CurrentValue.Infobip.BasePath,
        ApiKey = appSettings.CurrentValue.Infobip.ApiKey,
    });

    private string CompanyName => appSettings.CurrentValue.CompanyName;
    private string FromEmail => appSettings.CurrentValue.EmailService.FromEmail;
    private string HeaderImage => appSettings.CurrentValue.EmailService.HeaderImage;

    public async Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        try
        {
            var subject = $"Confirma tu email - {CompanyName}";
            var fullName = InfobipEmailSender<TUser>.GetFullName(user);
            var content = await emailTemplateService.RenderConfirmationEmailAsync(
                fullName,
                confirmationLink,
                HeaderImage);

            await SendEmailAsync(FromEmail, new List<string> { email }, subject, content);

            logger.LogInformation("Email de confirmación enviado exitosamente a {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar email de confirmación a {Email}", email);
            throw;
        }
    }

    public async Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
    {
        try
        {
            string subject = $"Restablecer contraseña - {CompanyName}";
            string? fullName = InfobipEmailSender<TUser>.GetFullName(user);
            string? content = await emailTemplateService.RenderPasswordResetEmailAsync(
                fullName,
                resetLink,
                HeaderImage);

            await SendEmailAsync(FromEmail, new List<string> { email }, subject, content);

            logger.LogInformation("Email de restablecimiento de contraseña enviado exitosamente a {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar email de restablecimiento de contraseña a {Email}", email);
            throw;
        }
    }

    public async Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        try
        {
            string subject = "Código de restablecimiento - QCode";
            string? fullName = InfobipEmailSender<TUser>.GetFullName(user);
            string? content = await emailTemplateService.RenderPasswordResetCodeEmailAsync(
                fullName,
                resetCode,
                HeaderImage);

            await SendEmailAsync(FromEmail, new List<string> { email }, subject, content);

            logger.LogInformation("Email de código de restablecimiento enviado exitosamente a {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar email de código de restablecimiento a {Email}", email);
            throw;
        }
    }

    public async Task SendEmailAsync(string emailFrom, IReadOnlyCollection<string> emailTo, string subject, string contentHtml, IReadOnlyCollection<string>? cc = null, IReadOnlyCollection<string>? bcc = null, IReadOnlyCollection<Dtos.EmailAttachmentDto>? attachment = null)
    {
        try
        {
            await _infobipClient.SendEmailAsync(
                from: emailFrom,
                to: emailTo.ToList(),
                subject: subject,
                html: contentHtml,
                cc: cc?.ToList(),
                bcc: bcc?.ToList(),
                attachment: attachment?.Select(a => new FileParameter(a.Content)).ToList()
                );
        }
        catch (Exception ex)
        {
            // Log del error con información detallada
            logger.LogError(ex, "Error al enviar email a {EmailTo} con asunto '{Subject}'",
                string.Join(", ", emailTo), subject);

            // Re-lanzar como una excepción más específica
            throw new InvalidOperationException($"Error al enviar email a {string.Join(", ", emailTo)}: {ex.Message}", ex);
        }
    }

    private static string GetFullName(TUser user)
    {
        string? firstName = user.GetType().GetProperty("FirstName")?.GetValue(user)?.ToString();
        string? lastName = user.GetType().GetProperty("LastName")?.GetValue(user)?.ToString();
        return $"{firstName} {lastName}";
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _infobipClient?.Dispose();
        }
    }
}
