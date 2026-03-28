using System.Security.Claims;
using Domain.AggregatesModel.UserAggregate;

namespace Domain.Interfaces;

/// <summary>
/// Servicio para generar y validar tokens JWT
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Genera un token de acceso JWT para un usuario
    /// </summary>
    /// <param name="user">Usuario para el cual generar el token</param>
    /// <param name="provider">Proveedor de autenticación</param>
    /// <returns>Token JWT como string</returns>
    string GenerateToken(User user, string provider);

    /// <summary>
    /// Genera un refresh token JWT para un usuario
    /// </summary>
    /// <param name="user">Usuario para el cual generar el refresh token</param>
    /// <returns>Refresh token JWT como string</returns>
    string GenerateRefreshToken(User user);

    /// <summary>
    /// Valida un token JWT de acceso
    /// </summary>
    /// <param name="token">Token JWT a validar</param>
    /// <returns>ClaimsPrincipal si el token es válido, null en caso contrario</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Valida un refresh token JWT
    /// </summary>
    /// <param name="refreshToken">Refresh token JWT a validar</param>
    /// <returns>ClaimsPrincipal si el refresh token es válido, null en caso contrario</returns>
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
}
