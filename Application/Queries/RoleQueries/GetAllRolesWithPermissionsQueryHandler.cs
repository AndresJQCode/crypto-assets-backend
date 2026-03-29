using Application.Dtos.Permission;
using Application.Dtos.Role;
using Domain.AggregatesModel.RoleAggregate;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.RoleQueries;

internal sealed class GetAllRolesWithPermissionsQueryHandler : IRequestHandler<GetAllRolesWithPermissionsQuery, IEnumerable<RoleWithPermissionsDto>>
{
    private readonly RoleManager<Role> _roleManager;

    public GetAllRolesWithPermissionsQueryHandler(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<RoleWithPermissionsDto>> Handle(GetAllRolesWithPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Optimización: Proyectar directamente en la consulta para evitar procesamiento en memoria
        var roles = await _roleManager.Roles
                .Include(r => r.PermissionRoles)
                        .ThenInclude(pr => pr.Permission)
                .OrderBy(r => r.Name)
                .Select(role => new RoleWithPermissionsDto
                {
                    Id = role.Id.ToString(),
                    Name = role.Name!,
                    Description = role.Description,
                    Permissions = role.PermissionRoles
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
                        .ToList()
                })
                .ToListAsync(cancellationToken);

        return roles;
    }
}
