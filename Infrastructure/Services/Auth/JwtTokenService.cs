using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.AggregatesModel.UserAggregate;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Auth;

public class JwtTokenService(
    IOptionsMonitor<AppSettings> appSettings,
    ILogger<JwtTokenService> logger) : IJwtTokenService
{
    private AppSettings.JwtConfiguration JwtSettings => appSettings.CurrentValue.JwtSettings;

    public string GenerateToken(User user, string provider)
    {
        try
        {
            var secretKey = JwtSettings.SecretKey;
            var issuer = JwtSettings.Issuer;
            var audience = JwtSettings.Audience;
            var expirationMinutes = JwtSettings.ExpirationMinutes;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
                        {
                                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                new(JwtRegisteredClaimNames.Iat,
                                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                                        ClaimValueTypes.Integer64)
                        };
            if (user.TenantId.HasValue)
                claims.Add(new Claim(AppConstants.Authentication.TenantIdClaim, user.TenantId.Value.ToString()));

            var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                    signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Métrica de Prometheus: token de acceso generado
            InfrastructureMetrics.JwtTokensGeneratedTotal
                .WithLabels(MetricsLabelsConstants.Jwt.Access)
                .Inc();

            return tokenString;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar JWT token para el usuario {UserId}", user.Id);
            throw;
        }
    }

    public string GenerateRefreshToken(User user)
    {
        try
        {
            var secretKey = JwtSettings.SecretKey;
            var issuer = JwtSettings.Issuer;
            var audience = JwtSettings.Audience;
            var refreshTokenExpirationDays = JwtSettings.RefreshTokenExpirationDays;

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey no está configurado");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                // Solo claims seguros y necesarios para refresh token
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new("token_type", "refresh"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Métrica de Prometheus: refresh token generado
            InfrastructureMetrics.JwtTokensGeneratedTotal
                .WithLabels(MetricsLabelsConstants.Jwt.Refresh)
                .Inc();

            return tokenString;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar refresh token para el usuario {UserId}", user.Id);
            throw;
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var secretKey = JwtSettings.SecretKey;
            var issuer = JwtSettings.Issuer;
            var audience = JwtSettings.Audience;

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey no está configurado");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Métrica de Prometheus: validación exitosa
            InfrastructureMetrics.JwtTokensGeneratedTotal
                .WithLabels(MetricsLabelsConstants.Jwt.ValidationSuccess)
                .Inc();

            return principal;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al validar JWT token");

            // Métrica de Prometheus: validación fallida
            InfrastructureMetrics.JwtTokensGeneratedTotal
                .WithLabels(MetricsLabelsConstants.Jwt.ValidationFailed)
                .Inc();

            return null;
        }
    }

    public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var secretKey = JwtSettings.SecretKey;
            var issuer = JwtSettings.Issuer;
            var audience = JwtSettings.Audience;

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey no está configurado");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);

            // Verificar que sea un refresh token
            var tokenType = principal.FindFirst("token_type")?.Value;
            if (tokenType != "refresh")
            {
                logger.LogWarning("Token no es un refresh token válido");

                // Métrica de Prometheus: validación fallida (tipo incorrecto)
                InfrastructureMetrics.JwtTokensGeneratedTotal
                    .WithLabels(MetricsLabelsConstants.Jwt.RefreshValidationInvalidType)
                    .Inc();

                return null;
            }

            // Métrica de Prometheus: validación exitosa de refresh token
            InfrastructureMetrics.JwtTokensGeneratedTotal
                .WithLabels(MetricsLabelsConstants.Jwt.RefreshValidationSuccess)
                .Inc();

            return principal;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al validar refresh token");

            // Métrica de Prometheus: validación fallida de refresh token
            InfrastructureMetrics.JwtTokensGeneratedTotal
                .WithLabels(MetricsLabelsConstants.Jwt.RefreshValidationFailed)
                .Inc();

            return null;
        }
    }
}
