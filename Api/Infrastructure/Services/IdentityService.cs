using System.Security.Claims;
using Domain.Interfaces;

namespace Api.Infrastructure.Services;

internal sealed class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor _context;

    public IdentityService(IHttpContextAccessor context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public int? GetUserIdentity()
    {
        string? idValue = _context.HttpContext?.User?.FindFirst("id")?.Value;
        return string.IsNullOrEmpty(idValue) ? null : int.Parse(idValue, System.Globalization.CultureInfo.InvariantCulture);
    }

    public string GetUserName()
    {
        return _context.HttpContext?.User?.Identity?.Name ?? string.Empty;
    }

    public Guid? GetCurrentUserId()
    {
        try
        {
            var userIdClaim = _context.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);
        }
        catch
        {
            return null;
        }
    }

    public string? GetCurrentUserName()
    {
        try
        {
            return _context.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                ?? _context.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
