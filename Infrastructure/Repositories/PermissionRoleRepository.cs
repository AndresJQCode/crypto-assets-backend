using Domain.AggregatesModel.PermissionAggregate;
using Domain.AggregatesModel.RoleAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class PermissionRoleRepository(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<Repository<PermissionRole>> logger)
    : Repository<PermissionRole>(context, httpContextAccessor, logger), IPermissionRoleRepository
{
    private readonly ApiContext _context = context;

    public async Task<IEnumerable<PermissionRole>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.PermissionRoles
            .Where(pr => pr.RoleId == roleId && pr.IsActive)
            .Include(pr => pr.Permission)
            .Include(pr => pr.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PermissionRole>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.PermissionRoles
            .Where(pr => pr.PermissionId == permissionId && pr.IsActive)
            .Include(pr => pr.Permission)
            .Include(pr => pr.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<PermissionRole?> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.PermissionRoles
            .Where(pr => pr.RoleId == roleId && pr.PermissionId == permissionId && pr.IsActive)
            .Include(pr => pr.Permission)
            .Include(pr => pr.Role)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.PermissionRoles
            .Where(pr => pr.RoleId == roleId && pr.IsActive)
            .Select(pr => pr.Permission)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetRolesByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.PermissionRoles
            .Where(pr => pr.PermissionId == permissionId && pr.IsActive)
            .Select(pr => pr.Role)
            .ToListAsync(cancellationToken);
    }
}
