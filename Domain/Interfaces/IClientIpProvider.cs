namespace Domain.Interfaces;

/// <summary>
/// Proporciona la dirección IP del cliente de la petición actual (cuando hay un contexto HTTP).
/// Permite a capas que no conocen ASP.NET Core obtener la IP para auditoría, etc.
/// </summary>
public interface IClientIpProvider
{
    /// <summary>
    /// Obtiene la IP del cliente en el contexto actual, o null si no hay contexto (p. ej. fuera de una petición HTTP).
    /// </summary>
    string? GetClientIp();
}
