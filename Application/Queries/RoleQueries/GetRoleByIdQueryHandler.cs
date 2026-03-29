using Application.Dtos.Role;
using Domain.AggregatesModel.RoleAggregate;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Queries.RoleQueries;

internal sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto?>
{
    private readonly RoleManager<Role> _roleManager;

    public GetRoleByIdQueryHandler(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<RoleDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());

        if (role == null)
        {
            return null;
        }

        return new RoleDto
        {
            Id = role.Id.ToString(),
            Name = role.Name!,
            Description = role.Description
        };
    }
}
