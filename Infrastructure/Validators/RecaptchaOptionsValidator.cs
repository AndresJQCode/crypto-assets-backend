using Microsoft.Extensions.Options;

namespace Infrastructure.Validators;

/// <summary>
/// Validador para la sección Recaptcha de la configuración.
/// Se ejecuta al inicio de la aplicación cuando reCAPTCHA está habilitado.
/// </summary>
public class RecaptchaOptionsValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        var recaptcha = options.Recaptcha;

        if (!recaptcha.Enabled)
            return ValidateOptionsResult.Success;

        if (string.IsNullOrWhiteSpace(recaptcha.ProjectId))
        {
            return ValidateOptionsResult.Fail(
                "Recaptcha:ProjectId es requerido cuando Recaptcha está habilitado (reCAPTCHA Enterprise).");
        }

        if (string.IsNullOrWhiteSpace(recaptcha.SiteKey))
        {
            return ValidateOptionsResult.Fail(
                "Recaptcha:SiteKey es requerido cuando Recaptcha está habilitado.");
        }

        if (recaptcha.MinimumScore is < 0.0 or > 1.0)
        {
            return ValidateOptionsResult.Fail(
                $"Recaptcha:MinimumScore debe estar entre 0.0 y 1.0. Valor actual: {recaptcha.MinimumScore}");
        }

        if (recaptcha.TimeoutSeconds < 1 || recaptcha.TimeoutSeconds > 60)
        {
            return ValidateOptionsResult.Fail(
                $"Recaptcha:TimeoutSeconds debe estar entre 1 y 60 segundos. Valor actual: {recaptcha.TimeoutSeconds}");
        }

        return ValidateOptionsResult.Success;
    }
}
