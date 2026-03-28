using Api.Application.Dtos.Permission;
using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Api.Application.Queries.PermissionQueries
{
    internal sealed class GetPermissionsByUserIdQueryHandler : IRequestHandler<GetPermissionsByUserIdQuery, IEnumerable<PermissionDto>>
    {
        private readonly IPermissionRepository _permissionRepository;

        public GetPermissionsByUserIdQueryHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<IEnumerable<PermissionDto>> Handle(GetPermissionsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(request.UserId);

            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Resource = p.Resource,
                Action = p.Action
            });
        }
    }
}
