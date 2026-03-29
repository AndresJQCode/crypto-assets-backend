namespace Application.Services.Auth;

/// <summary>
/// Configuración para el flujo OAuth (Google, Microsoft).
/// </summary>
internal sealed class OAuthSettings
{
    /// <summary>
    /// Si es true, permite que usuarios nuevos se registren al iniciar sesión con proveedor externo (FindOrCreateUser).
    /// Si es false, solo se permite el acceso a usuarios ya existentes; si no existe, se retorna 401.
    /// </summary>
    public bool AllowPublicUsers { get; set; }
}
