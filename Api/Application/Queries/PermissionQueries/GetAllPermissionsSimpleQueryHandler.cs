using Api.Application.Dtos.Permission;
using Domain.AggregatesModel.PermissionAggregate;
using MediatR;

namespace Api.Application.Queries.PermissionQueries;

internal sealed class GetAllPermissionsSimpleQueryHandler : IRequestHandler<GetAllPermissionsSimpleQuery, IEnumerable<PermissionDto>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetAllPermissionsSimpleQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<IEnumerable<PermissionDto>> Handle(GetAllPermissionsSimpleQuery request, CancellationToken cancellationToken)
    {
        // Obtener todos los permisos
        var allPermissions = await _permissionRepository.GetByFilter();
        var query = allPermissions.AsQueryable();

        // Aplicar filtros específicos si se proporcionan

        if (!string.IsNullOrEmpty(request.Resource))
        {
            query = query.Where(p => p.Resource == request.Resource);
        }

        // Ordenar por nombre
        var permissions = query.OrderBy(p => p.Name).ToList();

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
