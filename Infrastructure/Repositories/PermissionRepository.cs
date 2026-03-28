using Domain.AggregatesModel.PermissionAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class PermissionRepository(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<Repository<Permission>> logger)
    : Repository<Permission>(context, httpContextAccessor, logger), IPermissionRepository
{
    private readonly ApiContext _context = context;


    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource)
    {
        return await _context.Permissions
            .Where(p => p.Resource == resource)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetActivePermissionsAsync()
    {
        return await _context.Permissions
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId)
    {
        // Obtener permisos del usuario a través de sus roles
        return await _context.PermissionRoles
            .Where(pr => pr.IsActive)
            .Where(pr => _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .Any(roleId => roleId == pr.RoleId))
            .Select(pr => pr.Permission)
            .Distinct()
            .ToListAsync();
    }
}
