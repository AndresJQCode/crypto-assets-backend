using Infrastructure.Constants;

namespace Api.Utilities;

/// <summary>
/// Claves de caché para el flujo de reseteo de contraseña.
/// </summary>
internal static class PasswordResetCacheKeys
{
    /// <summary>
    /// Obtiene la clave de caché del tracker de intentos para un email.
    /// </summary>
    public static string GetTrackerKey(string email) =>
        CacheKeyConstants.PasswordReset.GetTrackerKey(email);
}
