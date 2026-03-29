using System.Diagnostics;
using Domain.AggregatesModel.PermissionAggregate;
using Infrastructure;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    internal sealed class UserPermissionService(ApiContext context) : IUserPermissionService
    {

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Obtener permisos a través de roles únicamente
                var permissions = await GetUserPermissionsByRolesAsync(userId, cancellationToken);

                stopwatch.Stop();

                // Métrica de Prometheus: query exitoso
                InfrastructureMetrics.DatabaseQueriesTotal
                    .WithLabels(MetricsLabelsConstants.Database.Select, "permissions", MetricsLabelsConstants.Database.Success)
                    .Inc();

                InfrastructureMetrics.DatabaseQueryDuration
                    .WithLabels(MetricsLabelsConstants.Database.Select, "permissions")
                    .Observe(stopwatch.Elapsed.TotalSeconds);

                return permissions;
            }
            catch (Exception)
            {
                stopwatch.Stop();

                // Métrica de Prometheus: query fallido
                InfrastructureMetrics.DatabaseQueriesTotal
                    .WithLabels(MetricsLabelsConstants.Database.Select, "permissions", MetricsLabelsConstants.Database.Error)
                    .Inc();

                InfrastructureMetrics.DatabaseErrorsTotal
                    .WithLabels("query_error", "permissions")
                    .Inc();

                throw;
            }
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsByRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await context.PermissionRoles
                .Where(pr => pr.IsActive)
                .Where(pr => context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .Contains(pr.RoleId))
                .Select(pr => pr.Permission)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        public Task<IEnumerable<Permission>> GetUserDirectPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Ya no hay permisos directos, solo por roles
            return Task.FromResult<IEnumerable<Permission>>(new List<Permission>());
        }

        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
                var hasPermission = userPermissions.Any(p => p.PermissionKey == permissionKey);

                stopwatch.Stop();

                // Métrica de Prometheus: verificación de permiso
                InfrastructureMetrics.PermissionChecksTotal
                    .WithLabels(permissionKey, hasPermission ? "allowed" : "denied")
                    .Inc();

                InfrastructureMetrics.PermissionCheckDuration
                    .WithLabels(permissionKey, "service")
                    .Observe(stopwatch.Elapsed.TotalSeconds);

                return hasPermission;
            }
            catch (Exception)
            {
                stopwatch.Stop();

                // Métrica de Prometheus: error en verificación de permiso
                InfrastructureMetrics.PermissionChecksTotal
                    .WithLabels(permissionKey, "error")
                    .Inc();

                throw;
            }
        }

        private sealed class PermissionEqualityComparer : IEqualityComparer<Permission>
        {
            public bool Equals(Permission? x, Permission? y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(Permission obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
