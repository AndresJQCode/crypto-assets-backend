using Domain.AggregatesModel.PermissionAggregate;

namespace Application.Services;

internal interface IUserPermissionService
{
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetUserPermissionsByRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetUserDirectPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    // Task<bool> UserHasPermissionAsync(Guid userId, string resource, string action, CancellationToken cancellationToken = default);
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken = default);
}
