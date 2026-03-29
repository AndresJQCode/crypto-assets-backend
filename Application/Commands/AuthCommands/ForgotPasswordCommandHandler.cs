using System.Text;
using System.Text.Json;
using Application.Dtos.Auth;
using Api.Extensions;
using Domain.AggregatesModel.AuditAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Interfaces;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Api.Utilities;

namespace Application.Commands.AuthCommands;

/// <summary>
/// Clase para rastrear los intentos de envío de códigos de reseteo de contraseña
/// </summary>
internal sealed class PasswordResetAttemptTracker
{
    public DateTime LastSentTime { get; set; }
    public int AttemptCount { get; set; }
}

internal sealed class ForgotPasswordCommandHandler(
                UserManager<User> userManager,
                IEmailSender<User> emailSender,
                IOptionsMonitor<AppSettings> appSettings,
                IRecaptchaService recaptchaService,
                IHttpContextAccessor httpContextAccessor,
                ICacheService cacheService,
                ApiContext context) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponseDto>
{
    public async Task<ForgotPasswordResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Validar reCAPTCHA si está habilitado
        if (appSettings.CurrentValue.Recaptcha.Enabled && appSettings.CurrentValue.Recaptcha.RequiresValidation("/auth/forgotPassword"))
        {
            var remoteIp = httpContextAccessor.HttpContext?.GetClientIpAddress();
            var recaptchaResult = await recaptchaService.ValidateTokenAsync(request.RecaptchaToken ?? string.Empty, remoteIp);

            if (!recaptchaResult.Success)
            {
                throw new Domain.Exceptions.BadRequestException($"Validación de reCAPTCHA fallida: {recaptchaResult.ErrorMessage}");
            }
        }
        User? user = await userManager.FindByEmailAsync(request.Email);

        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            PasswordResetAttemptTracker? tracker = null;

            // Verificar intervalo permitido antes de enviar el código
            if (appSettings.CurrentValue.PasswordReset.Enabled)
            {
                var cacheKey = PasswordResetCacheKeys.GetTrackerKey(request.Email);
                tracker = cacheService.Get<PasswordResetAttemptTracker>(cacheKey);

                if (tracker != null)
                {
                    // El AttemptCount actual representa el número de intentos previos
                    // Para el siguiente intento, usamos AttemptCount (que es el número de intento siguiente)
                    var requiredIntervalSeconds = appSettings.CurrentValue.PasswordReset.GetIntervalForAttempt(tracker.AttemptCount);
                    var requiredInterval = TimeSpan.FromSeconds(requiredIntervalSeconds);
                    var timeSinceLastSent = DateTime.UtcNow - tracker.LastSentTime;

                    if (timeSinceLastSent < requiredInterval)
                    {
                        var remainingTime = requiredInterval - timeSinceLastSent;
                        var remainingSeconds = (int)Math.Ceiling(remainingTime.TotalSeconds);

                        string timeMessage;
                        if (remainingSeconds < 60)
                        {
                            // Si falta menos de 1 minuto, mostrar segundos
                            timeMessage = $"{remainingSeconds} segundos";
                        }
                        else if (remainingSeconds < 3600)
                        {
                            // Mostrar minutos como entero
                            var minutes = (int)Math.Ceiling(remainingSeconds / 60.0);
                            timeMessage = $"{minutes} minuto(s) y {remainingSeconds % 60} segundos";
                        }
                        else
                        {
                            // Mostrar horas como entero
                            var hours = (int)Math.Ceiling(remainingSeconds / 3600.0);
                            timeMessage = $"{hours} hora(s), {remainingSeconds % 3600} minuto(s) y {remainingSeconds % 60} segundos";
                        }

                        throw new Domain.Exceptions.BadRequestException(
                            $"Debes esperar {timeMessage} antes de solicitar otro código de reseteo de contraseña.");
                    }

                    // Actualizar la fecha del último envío (el contador se incrementa después del envío)
                    tracker.LastSentTime = DateTime.UtcNow;
                }
                else
                {
                    // Primer intento: AttemptCount = 0 (puede enviar inmediatamente)
                    // Después del envío será 1 (segundo intento requerirá 30 segundos)
                    tracker = new PasswordResetAttemptTracker
                    {
                        AttemptCount = 0,
                        LastSentTime = DateTime.UtcNow
                    };
                }
            }

            // Generar y enviar el código (siempre se envía si pasa la validación)
            string? code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Crear enlace de restablecimiento usando la URL del frontend configurada
            string? frontUrl = appSettings.CurrentValue.FrontUrl.TrimEnd('/');
            string? resetLink = $"{frontUrl}/reset-password?code={code}&email={Uri.EscapeDataString(request.Email)}";

            await emailSender.SendPasswordResetLinkAsync(user, request.Email, resetLink);

            // Guardar el tracker actualizado en caché (si la validación está habilitada)
            if (appSettings.CurrentValue.PasswordReset.Enabled && tracker != null)
            {
                var cacheKey = PasswordResetCacheKeys.GetTrackerKey(request.Email);
                // Incrementar el contador después del envío para el próximo intento
                tracker.AttemptCount++;
                // El tiempo de expiración debe ser al menos el intervalo máximo más un margen
                var maxIntervalSeconds = appSettings.CurrentValue.PasswordReset.IncrementalIntervalsSeconds.Max();
                var expiration = TimeSpan.FromSeconds(maxIntervalSeconds + 3600); // Agregar 1 hora de margen
                cacheService.Set(cacheKey, tracker, expiration);
            }

            // Registrar en auditoría
            var remoteIp = httpContextAccessor.HttpContext?.GetClientIpAddress();
            var additionalData = JsonSerializer.Serialize(new
            {
                Email = request.Email,
                RemoteIpAddress = remoteIp,
            });

            var auditLog = new AuditLog(
                entityType: AppConstants.AuditEntityTypes.User,
                entityId: user.Id,
                action: AppConstants.AuditActions.PasswordResetRequested,
                userId: user.Id,
                userName: user.Email,
                reason: "Solicitud de reseteo de contraseña",
                additionalData: additionalData
            );

            context.AuditLogs.Add(auditLog);
            await context.SaveEntitiesAsync(cancellationToken);
        }

        // Por seguridad, siempre devolver OK para no revelar si el email existe
        return new ForgotPasswordResponseDto { Message = "Si el email existe, se ha enviado un enlace para restablecer la contraseña." };
    }
}
