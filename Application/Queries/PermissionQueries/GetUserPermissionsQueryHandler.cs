using Application.Services;
using MediatR;

namespace Application.Queries.PermissionQueries;

internal sealed class GetUserPermissionsQueryHandler(IUserPermissionService userPermissionService) : IRequestHandler<GetUserPermissionsQuery, IEnumerable<Domain.AggregatesModel.PermissionAggregate.Permission>>
{
    private readonly IUserPermissionService _userPermissionService = userPermissionService;

    public async Task<IEnumerable<Domain.AggregatesModel.PermissionAggregate.Permission>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Solo se obtienen permisos por roles
        return await _userPermissionService.GetUserPermissionsByRolesAsync(request.UserId, cancellationToken);
    }
}
