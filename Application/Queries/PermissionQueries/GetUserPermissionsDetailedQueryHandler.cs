using Application.Dtos.Permission;
using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Application.Queries.PermissionQueries
{
    internal sealed class GetUserPermissionsDetailedQueryHandler : IRequestHandler<GetUserPermissionsDetailedQuery, IEnumerable<UserPermissionDto>>
    {
        private readonly IPermissionRepository _permissionRepository;

        public GetUserPermissionsDetailedQueryHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<IEnumerable<UserPermissionDto>> Handle(GetUserPermissionsDetailedQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(request.UserId);

            return permissions.Select(p => new UserPermissionDto
            {
                PermissionKey = p.PermissionKey,
                Resource = p.Resource,
                Action = p.Action
            });
        }
    }
}
