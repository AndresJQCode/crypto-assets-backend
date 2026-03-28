using Microsoft.Extensions.Options;

namespace Infrastructure.Validators;

/// <summary>
/// Validates EmailService configuration at startup using IValidateOptions pattern
/// </summary>
public class EmailServiceOptionsValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        var emailSettings = options.EmailService;

        if (emailSettings == null)
        {
            return ValidateOptionsResult.Fail(
                "EmailService configuration section is required in appsettings.json. " +
                "Please add the 'EmailService' section with required properties: " +
                "Provider, FromEmail, FromName, HeaderImage");
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(emailSettings.Provider))
            errors.Add("EmailService.Provider is required. Supported values: 'Infobip', 'SendGrid', 'Smtp'");

        if (string.IsNullOrWhiteSpace(emailSettings.FromEmail))
            errors.Add("EmailService.FromEmail is required (e.g., 'noreply@lulocrm.com')");

        if (string.IsNullOrWhiteSpace(emailSettings.FromName))
            errors.Add("EmailService.FromName is required (e.g., 'Lulo CRM')");

        if (string.IsNullOrWhiteSpace(emailSettings.HeaderImage))
            errors.Add("EmailService.HeaderImage is required (URL to email header logo)");

        // Validate email format
        if (!string.IsNullOrWhiteSpace(emailSettings.FromEmail) &&
            !IsValidEmail(emailSettings.FromEmail))
        {
            errors.Add($"EmailService.FromEmail '{emailSettings.FromEmail}' is not a valid email address");
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(
                $"EmailService configuration is invalid:{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors.Select(e => $"  - {e}")));
        }

        return ValidateOptionsResult.Success;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
