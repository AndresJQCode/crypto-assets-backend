namespace Domain.Interfaces;

/// <summary>
/// Main email service interface for business logic
/// Uses IEmailProvider strategy for actual email delivery
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send confirmation email with link
    /// </summary>
    Task SendConfirmationLinkAsync(string email, string fullName, string confirmationLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send password reset email with link
    /// </summary>
    Task SendPasswordResetLinkAsync(string email, string fullName, string resetLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send password reset email with code
    /// </summary>
    Task SendPasswordResetCodeAsync(string email, string fullName, string resetCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send custom email (for notifications, alerts, etc.)
    /// </summary>
    Task SendCustomEmailAsync(
        string from,
        IReadOnlyCollection<string> to,
        string subject,
        string htmlBody,
        IReadOnlyCollection<string>? cc = null,
        IReadOnlyCollection<string>? bcc = null,
        CancellationToken cancellationToken = default);
}
