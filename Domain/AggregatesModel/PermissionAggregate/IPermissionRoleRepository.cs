using Domain.AggregatesModel.RoleAggregate;
using Domain.SeedWork;

namespace Domain.AggregatesModel.PermissionAggregate
{
    public interface IPermissionRoleRepository : IRepository<PermissionRole>
    {
        Task<IEnumerable<PermissionRole>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PermissionRole>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
        Task<PermissionRole?> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetRolesByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
    }
}
