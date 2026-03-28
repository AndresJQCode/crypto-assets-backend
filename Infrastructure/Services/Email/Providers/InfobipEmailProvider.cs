using Domain.Interfaces;
using Infobip.Api.Client;
using Infobip.Api.Client.Api;
using Infobip.Api.Client.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Email.Providers;

/// <summary>
/// Infobip email provider implementation
/// </summary>
public class InfobipEmailProvider : IEmailProvider, IDisposable
{
    private readonly EmailApi _infobipClient;
    private readonly ILogger<InfobipEmailProvider> _logger;

    public string ProviderName => "Infobip";

    public InfobipEmailProvider(
        IOptionsMonitor<AppSettings> appSettings,
        ILogger<InfobipEmailProvider> logger)
    {
        _logger = logger;
        _infobipClient = new EmailApi(new Configuration
        {
            BasePath = appSettings.CurrentValue.Infobip.BasePath,
            ApiKey = appSettings.CurrentValue.Infobip.ApiKey,
        });
    }

    public async Task SendEmailAsync(
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
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Sending email via {Provider} to {Recipients}",
                    ProviderName,
                    string.Join(", ", to));
            }

            await _infobipClient.SendEmailAsync(
                from: from,
                to: to.ToList(),
                subject: subject,
                html: htmlBody,
                cc: cc?.ToList(),
                bcc: bcc?.ToList(),
                cancellationToken: cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Email sent successfully via {Provider} to {Recipients}",
                    ProviderName,
                    string.Join(", ", to));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending email via {Provider} to {Recipients} with subject '{Subject}'",
                ProviderName,
                string.Join(", ", to),
                subject);

            throw new InvalidOperationException(
                $"Error al enviar email via {ProviderName} a {string.Join(", ", to)}: {ex.Message}",
                ex);
        }
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
