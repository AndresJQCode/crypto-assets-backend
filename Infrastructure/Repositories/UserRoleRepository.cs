using Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRoleRepository(ApiContext context) : IUserRoleRepository
{
    public async Task<List<Guid>> GetUserIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken);
    }
}
