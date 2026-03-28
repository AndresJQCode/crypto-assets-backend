using Microsoft.Extensions.Options;

namespace Infrastructure.Validators;

/// <summary>
/// Validates Infobip configuration when Provider is set to "Infobip"
/// </summary>
public class InfobipOptionsValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        // Only validate if Infobip is the selected provider
        var provider = options.EmailService?.Provider?.ToUpperInvariant();
        if (provider != "INFOBIP")
        {
            return ValidateOptionsResult.Success; // Not using Infobip, skip validation
        }

        var infobipSettings = options.Infobip;

        if (infobipSettings == null)
        {
            return ValidateOptionsResult.Fail(
                "Infobip configuration section is required in appsettings.json " +
                "because EmailService.Provider is set to 'Infobip'. " +
                "Please add the 'Infobip' section with properties: BasePath, ApiKey");
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(infobipSettings.BasePath))
            errors.Add("Infobip.BasePath is required (e.g., 'https://api.infobip.com')");

        if (string.IsNullOrWhiteSpace(infobipSettings.ApiKey))
            errors.Add("Infobip.ApiKey is required (your Infobip API key)");

        // Validate BasePath is a valid URL
        if (!string.IsNullOrWhiteSpace(infobipSettings.BasePath) &&
            !Uri.TryCreate(infobipSettings.BasePath, UriKind.Absolute, out var uri))
        {
            errors.Add($"Infobip.BasePath '{infobipSettings.BasePath}' is not a valid URL");
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(
                $"Infobip configuration is invalid:{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors.Select(e => $"  - {e}")));
        }

        return ValidateOptionsResult.Success;
    }
}
