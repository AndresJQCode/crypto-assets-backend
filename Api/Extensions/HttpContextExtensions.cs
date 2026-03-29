using Api.Constants;

namespace Api.Extensions;

internal static class HttpContextExtensions
{
    private const string IPv6Loopback = "::1";
    private const string IPv4Loopback = "127.0.0.1";

    /// <summary>
    /// Obtiene la IP del cliente, respetando X-Forwarded-For/X-Real-IP cuando la app está detrás de un proxy,
    /// y normalizando ::1 (localhost IPv6) a 127.0.0.1.
    /// </summary>
    public static string? GetClientIpAddress(this HttpContext? context)
    {
        if (context is null)
        {
            return null;
        }

        // Detrás de proxy: usar la cabecera enviada por el proxy (primer IP de X-Forwarded-For o X-Real-IP)
        var forwardedFor = context.Request.Headers[HeaderConstants.IpAddress.ForwardedFor].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For puede ser "client, proxy1, proxy2"; nos interesa la primera (cliente)
            var clientIp = forwardedFor.Split(',', StringSplitOptions.TrimEntries).FirstOrDefault();
            return NormalizeLoopback(clientIp);
        }

        var realIp = context.Request.Headers[HeaderConstants.IpAddress.RealIp].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return NormalizeLoopback(realIp);
        }

        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        return NormalizeLoopback(remoteIp);
    }

    private static string? NormalizeLoopback(string? ip)
    {
        if (string.IsNullOrEmpty(ip))
        {
            return ip;
        }

        // Normalizar ::1 (IPv6 localhost) a 127.0.0.1 para consistencia en logs/auditoría
        if (ip == IPv6Loopback)
        {
            return IPv4Loopback;
        }

        return ip;
    }
}
