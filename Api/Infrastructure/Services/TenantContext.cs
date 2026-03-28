using System.Security.Claims;
using Infrastructure;

namespace Api.Infrastructure.Services;

internal sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid? GetCurrentTenantId()
    {
        try
        {
            var value = httpContextAccessor.HttpContext?.User?.FindFirst(AppConstants.Authentication.TenantIdClaim)?.Value;
            return string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var id) ? null : id;
        }
        catch
        {
            return null;
        }
    }
}
