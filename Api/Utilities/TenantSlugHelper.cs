using System.Text.RegularExpressions;

namespace Api.Utilities;

/// <summary>
/// Helper para generar y normalizar slugs de tenant (URL-friendly a partir del nombre).
/// </summary>
internal static class TenantSlugHelper
{
    private const string DefaultSlugWhenEmpty = "tenant";

    /// <summary>
    /// Genera un slug a partir del nombre del tenant: reemplaza caracteres no alfanuméricos por guiones,
    /// recorta guiones al inicio/final y devuelve en minúsculas. Si el resultado es vacío, devuelve <see cref="DefaultSlugWhenEmpty"/>.
    /// </summary>
    /// <param name="tenantName">Nombre del tenant (ej. "Mi Empresa S.A.").</param>
    /// <returns>Slug normalizado (ej. "mi-empresa-s-a") o "tenant" si queda vacío.</returns>
    public static string GenerateFromName(string? tenantName)
    {
        if (string.IsNullOrWhiteSpace(tenantName))
            return DefaultSlugWhenEmpty;

#pragma warning disable CA1308 // Slug para URLs se normaliza en minúsculas
        var slug = Regex.Replace(tenantName.Trim(), @"[^a-zA-Z0-9]+", "-").Trim('-').ToLowerInvariant();
#pragma warning restore CA1308

        return string.IsNullOrEmpty(slug) ? DefaultSlugWhenEmpty : slug;
    }

    /// <summary>
    /// Añade un sufijo aleatorio de 3 caracteres al slug para evitar colisiones (ej. "mi-empresa" → "mi-empresa-a1b").
    /// </summary>
    public static string AppendRandomSuffix(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return DefaultSlugWhenEmpty;
        return string.Concat(slug, "-", Guid.NewGuid().ToString("N").AsSpan(0, 3));
    }
}
