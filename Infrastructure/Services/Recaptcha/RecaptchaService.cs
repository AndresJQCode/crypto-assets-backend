using Domain.Interfaces;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.RecaptchaEnterprise.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Recaptcha;

/// <summary>
/// Servicio para validar tokens con Google reCAPTCHA Enterprise (recomendado por Google).
/// </summary>
public class RecaptchaService(
    IOptionsMonitor<AppSettings> appSettings,
    ILogger<RecaptchaService> logger) : IRecaptchaService
{
    private Task<RecaptchaEnterpriseServiceClient>? _clientTask;
    private readonly object _clientLock = new();

    private AppSettings.RecaptchaSettings Settings => appSettings.CurrentValue.Recaptcha;

    private Task<RecaptchaEnterpriseServiceClient> GetOrCreateClientAsync()
    {
        if (_clientTask != null)
            return _clientTask;
        lock (_clientLock)
        {
            _clientTask ??= CreateClientAsync();
            return _clientTask;
        }
    }

    private async Task<RecaptchaEnterpriseServiceClient> CreateClientAsync()
    {
        var recaptcha = Settings;

        if (!string.IsNullOrWhiteSpace(recaptcha.CredentialsJson))
        {
            GoogleCredential credential = CredentialFactory.FromJson<ServiceAccountCredential>(recaptcha.CredentialsJson)
                .ToGoogleCredential();
            var builder = new RecaptchaEnterpriseServiceClientBuilder
            {
                GoogleCredential = credential
            };
            return await builder.BuildAsync();
        }

        // 3) Application Default Credentials
        return await RecaptchaEnterpriseServiceClient.CreateAsync();
    }

    public async Task<RecaptchaValidationResult> ValidateTokenAsync(string recaptchaToken, string recaptchaAction)
    {
        RecaptchaEnterpriseServiceClient client = await GetOrCreateClientAsync();

        ProjectName projectName = new ProjectName(Settings.ProjectId);

        // Build the assessment request.
        CreateAssessmentRequest createAssessmentRequest = new CreateAssessmentRequest()
        {
            Assessment = new Assessment()
            {
                // Set the properties of the event to be tracked.
                Event = new Event()
                {
                    SiteKey = Settings.SiteKey,
                    Token = recaptchaToken,
                    ExpectedAction = recaptchaAction
                },
            },
            ParentAsProjectName = projectName
        };

        Assessment response = await client.CreateAssessmentAsync(createAssessmentRequest);

        // Check if the token is valid.
        if (response.TokenProperties.Valid == false)
        {
            System.Console.WriteLine("The CreateAssessment call failed because the token was: " +
                response.TokenProperties.InvalidReason.ToString());
            return new RecaptchaValidationResult
            {
                Success = false,
                Score = 0.0,
                ErrorMessage = "Token de reCAPTCHA inválido",
                ErrorCodes = new List<string> { "invalid-token" }
            };
        }

        // Check if the expected action was executed.
        if (response.TokenProperties.Action != recaptchaAction)
        {
            System.Console.WriteLine("The action attribute in reCAPTCHA tag is: " +
                response.TokenProperties.Action.ToString());
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("The action attribute in the reCAPTCHA tag does not match the action you are expecting to score");
            }

            return new RecaptchaValidationResult
            {
                Success = false,
                Score = 0.0,
                ErrorMessage = "La acción del token no coincide con la esperada",
                ErrorCodes = new List<string> { "action-mismatch" }
            };
        }

        // Get the risk score and the reason(s).
        // For more information on interpreting the assessment, see:
        // https://cloud.google.com/recaptcha/docs/interpret-assessment

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Validación de reCAPTCHA Enterprise exitosa. Score: {Score}", (decimal)response.RiskAnalysis.Score);
        }

        double score = response.RiskAnalysis.Score;

        logger.LogWarning("Score de reCAPTCHA demasiado bajo: {Score} (mínimo requerido: {MinimumScore})",
            score, Settings.MinimumScore);

        if (score < Settings.MinimumScore)
        {
            List<string>? reasons = [];
            string? errorMessage = $"Score de reCAPTCHA demasiado bajo: {score:F2} (mínimo: {Settings.MinimumScore:F2})";

            foreach (RiskAnalysis.Types.ClassificationReason reason in response.RiskAnalysis.Reasons)
            {
                reasons.Add(reason.ToString());
            }

            return new RecaptchaValidationResult
            {
                Success = false,
                Score = score,
                ErrorMessage = errorMessage,
                ErrorCodes = reasons
            };
        }

        return new RecaptchaValidationResult
        {
            Success = true,
            Score = score,
            ErrorCodes = new List<string>()
        };
    }
}
