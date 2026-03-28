using Api.Extensions;
using Domain.Interfaces;

namespace Api.Infrastructure.Services;

internal sealed class ClientIpProvider(IHttpContextAccessor httpContextAccessor) : IClientIpProvider
{
    public string? GetClientIp() =>
        httpContextAccessor.HttpContext?.GetClientIpAddress();
}
