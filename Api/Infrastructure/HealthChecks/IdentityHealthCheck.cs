using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Infrastructure.HealthChecks;

internal sealed class IdentityHealthCheck : IHealthCheck
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<IdentityHealthCheck> _logger;

    public IdentityHealthCheck(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<IdentityHealthCheck> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificación simple: comprobar que Identity está accesible
            _ = _userManager.Users.Any();
            _ = _roleManager.Roles.Any();

            return Task.FromResult(HealthCheckResult.Healthy("Identity disponible"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar Identity");
            return Task.FromResult(HealthCheckResult.Unhealthy("Error en Identity", exception: ex));
        }
    }
}

