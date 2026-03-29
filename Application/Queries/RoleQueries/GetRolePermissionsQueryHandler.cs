using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Application.Queries.RoleQueries;

internal sealed class GetRolePermissionsQueryHandler(IPermissionRoleRepository permissionRoleRepository) : IRequestHandler<GetRolePermissionsQuery, IEnumerable<Permission>>
{
    private readonly IPermissionRoleRepository _permissionRoleRepository = permissionRoleRepository;

    public async Task<IEnumerable<Permission>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _permissionRoleRepository.GetPermissionsByRoleIdAsync(request.RoleId, cancellationToken);
    }
}
