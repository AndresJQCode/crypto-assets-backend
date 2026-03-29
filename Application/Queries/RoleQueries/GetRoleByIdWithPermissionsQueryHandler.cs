using Application.Dtos.Permission;
using Application.Dtos.Role;
using Domain.AggregatesModel.RoleAggregate;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.RoleQueries;

internal sealed class GetRoleByIdWithPermissionsQueryHandler : IRequestHandler<GetRoleByIdWithPermissionsQuery, RoleWithPermissionsDto?>
{
    private readonly RoleManager<Role> _roleManager;

    public GetRoleByIdWithPermissionsQueryHandler(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<RoleWithPermissionsDto?> Handle(GetRoleByIdWithPermissionsQuery request, CancellationToken cancellationToken)
    {
        Role? role = await _roleManager.Roles
                .Include(r => r.PermissionRoles)
                        .ThenInclude(pr => pr.Permission)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
        {
            return null;
        }

        var permissions = role.PermissionRoles
                        .Where(pr => pr.IsActive)
                        .Select(pr => new PermissionDto
                        {
                            Id = pr.Permission.Id,
                            Name = pr.Permission.Name,
                            Description = pr.Permission.Description,
                            Resource = pr.Permission.Resource,
                            Action = pr.Permission.Action,
                        })
                        .OrderBy(p => p.Resource)
                        .ThenBy(p => p.Action)
                        .ToList();

        return new RoleWithPermissionsDto
        {
            Id = role.Id.ToString(),
            Name = role.Name!,
            Description = role.Description,
            Permissions = permissions
        };
    }
}
