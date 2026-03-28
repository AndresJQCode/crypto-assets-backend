namespace Domain.Interfaces;

/// <summary>
/// Servicio para obtener información de identidad del usuario actual
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Obtiene el ID del usuario actual como entero
    /// </summary>
    /// <returns>ID del usuario o null si no está autenticado</returns>
    int? GetUserIdentity();

    /// <summary>
    /// Obtiene el nombre del usuario actual
    /// </summary>
    /// <returns>Nombre del usuario</returns>
    string GetUserName();

    /// <summary>
    /// Obtiene el ID del usuario actual como Guid
    /// </summary>
    /// <returns>ID del usuario como Guid o null si no está autenticado</returns>
    Guid? GetCurrentUserId();

    /// <summary>
    /// Obtiene el nombre o email del usuario actual
    /// </summary>
    /// <returns>Nombre o email del usuario, o null si no está autenticado</returns>
    string? GetCurrentUserName();
}
