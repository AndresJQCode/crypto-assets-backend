using System.Security.Claims;
using Application.Dtos.Dashboard;
using Application.Services;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.DashboardQueries;

internal sealed class GetDashboardMetricsQueryHandler(IHttpContextAccessor httpContextAccessor, IUserPermissionService userPermissionService, UserManager<User> userManager) : IRequestHandler<GetDashboardMetricsQuery, IEnumerable<DashboardMetricsDto>>
{

    public async Task<IEnumerable<DashboardMetricsDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {

        // obtner permisos del usuario logueado
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnAuthorizedException();
        }
        IEnumerable<Permission>? loggedUserPermissions = await userPermissionService.GetUserPermissionsAsync(userId, cancellationToken);

        List<DashboardMetricsDto> dashboardMetrics = [];
        if (loggedUserPermissions.Any(p => p.Resource == "Dashboard" && p.Action == "ViewAllUsersCount"))
        {
            List<User> allUsers = await userManager.Users.ToListAsync(cancellationToken);
            dashboardMetrics.Add(new DashboardMetricsDto { Name = "Total de Usuarios", Value = allUsers.Count, Icon = "Users", Color = "blue" });
        }
        if (loggedUserPermissions.Any(p => p.Resource == "Dashboard" && p.Action == "ViewActiveUsersCount"))
        {
            List<User> activeUsers = await userManager.Users.Where(u => u.IsActive).ToListAsync(cancellationToken);
            dashboardMetrics.Add(new DashboardMetricsDto { Name = "Usuarios Activos", Value = activeUsers.Count, Icon = "UserCheck", Color = "green" });
        }
        if (loggedUserPermissions.Any(p => p.Resource == "Dashboard" && p.Action == "ViewInactiveUsersCount"))
        {
            List<User> inactiveUsers = await userManager.Users.Where(u => !u.IsActive).ToListAsync(cancellationToken);
            dashboardMetrics.Add(new DashboardMetricsDto { Name = "Usuarios Inactivos", Value = inactiveUsers.Count, Icon = "UserX", Color = "red" });
        }
        return dashboardMetrics;
    }
}
