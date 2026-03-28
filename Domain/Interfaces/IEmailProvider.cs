namespace Domain.Interfaces;

/// <summary>
/// Interface for email provider implementations (Infobip, SendGrid, SMTP, etc.)
/// Strategy pattern - allows switching between different email providers
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Provider name (e.g., "Infobip", "SendGrid", "SMTP")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Send email using the provider's API
    /// </summary>
    /// <param name="from">Sender email address</param>
    /// <param name="to">Recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">Email HTML body</param>
    /// <param name="cc">CC recipients (optional)</param>
    /// <param name="bcc">BCC recipients (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(
        string from,
        IReadOnlyCollection<string> to,
        string subject,
        string htmlBody,
        IReadOnlyCollection<string>? cc = null,
        IReadOnlyCollection<string>? bcc = null,
        CancellationToken cancellationToken = default);
}
