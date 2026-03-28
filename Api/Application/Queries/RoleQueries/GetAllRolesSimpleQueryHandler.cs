using Api.Application.Dtos.Role;
using Domain.AggregatesModel.RoleAggregate;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.RoleQueries;

internal sealed class GetAllRolesSimpleQueryHandler : IRequestHandler<GetAllRolesSimpleQuery, IEnumerable<RoleDto>>
{
    private readonly RoleManager<Role> _roleManager;

    public GetAllRolesSimpleQueryHandler(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<RoleDto>> Handle(GetAllRolesSimpleQuery request, CancellationToken cancellationToken)
    {
        List<Role>? roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(selector: role => new RoleDto
        {
            Id = role.Id.ToString(),
            Name = role.Name!,
            Description = role.Description
        });
    }
}
