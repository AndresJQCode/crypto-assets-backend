using Api.Application.Dtos.Permission;
using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Api.Application.Queries.PermissionQueries
{
    internal sealed class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, PermissionDto?>
    {
        private readonly IPermissionRepository _permissionRepository;

        public GetPermissionByIdQueryHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<PermissionDto?> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
        {
            Permission? permission = await _permissionRepository.GetById(request.Id);

            if (permission == null)
                return null;

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Resource = permission.Resource,
                Action = permission.Action
            };
        }
    }
}
