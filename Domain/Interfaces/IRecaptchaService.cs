namespace Domain.Interfaces;

/// <summary>
/// Servicio para validar tokens de Google reCAPTCHA
/// </summary>
public interface IRecaptchaService
{
    /// <summary>
    /// Validar un token de reCAPTCHA
    /// </summary>
    /// <param name="recaptchaToken">Token generado por el frontend</param>
    /// <param name="recaptchaAction">Acción esperada en el token</param>
    /// <returns>Resultado de la validación con score y éxito</returns>
    Task<RecaptchaValidationResult> ValidateTokenAsync(string recaptchaToken, string recaptchaAction);
}

/// <summary>
/// Resultado de la validación de reCAPTCHA
/// </summary>
public class RecaptchaValidationResult
{
    private IReadOnlyCollection<string> _errorCodes = Array.Empty<string>();

    public bool Success { get; set; }
    public double Score { get; set; }
    public string? ErrorMessage { get; set; }

    public IReadOnlyCollection<string> ErrorCodes
    {
        get => _errorCodes;
        init => _errorCodes = value?.ToList().AsReadOnly() ?? (IReadOnlyCollection<string>)Array.Empty<string>();
    }
}
