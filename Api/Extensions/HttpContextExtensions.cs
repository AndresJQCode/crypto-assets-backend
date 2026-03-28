using System.Net;
using Api.Constants;

namespace Api.Extensions;

internal static class HttpContextExtensions
{
    private const string IPv6Loopback = "::1";
    private const string IPv4Loopback = "127.0.0.1";

    /// <summary>
    /// Obtiene la IP del cliente, respetando X-Forwarded-For/X-Real-IP cuando la app está detrás de un proxy.
    /// Normaliza a IPv4 cuando es posible: ::1 → 127.0.0.1 y direcciones IPv4-mapeadas (::ffff:x.x.x.x) → x.x.x.x.
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
            var clientIp = forwardedFor.Split(',', StringSplitOptions.TrimEntries).FirstOrDefault();
            return NormalizeToPreferredIp(clientIp);
        }

        var realIp = context.Request.Headers[HeaderConstants.IpAddress.RealIp].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return NormalizeToPreferredIp(realIp);
        }

        // Connection.RemoteIpAddress: preferir IPv4 si es dirección IPv4 mapeada en IPv6
        var remoteAddr = context.Connection.RemoteIpAddress;
        if (remoteAddr is not null)
        {
            if (remoteAddr.IsIPv4MappedToIPv6)
                return remoteAddr.MapToIPv4().ToString();
            return NormalizeToPreferredIp(remoteAddr.ToString());
        }

        return null;
    }

    /// <summary>
    /// Normaliza la IP: ::1 → 127.0.0.1; ::ffff:x.x.x.x → x.x.x.x (IPv4); resto sin cambio.
    /// </summary>
    private static string? NormalizeToPreferredIp(string? ip)
    {
        if (string.IsNullOrEmpty(ip))
            return ip;

        if (ip == IPv6Loopback)
            return IPv4Loopback;

        if (IPAddress.TryParse(ip, out var addr) && addr.IsIPv4MappedToIPv6)
            return addr.MapToIPv4().ToString();

        return ip;
    }
}
