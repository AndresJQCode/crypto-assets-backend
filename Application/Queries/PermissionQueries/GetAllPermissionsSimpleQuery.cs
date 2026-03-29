using Application.Dtos.Permission;
using MediatR;

namespace Application.Queries.PermissionQueries;

internal sealed class GetAllPermissionsSimpleQuery : IRequest<IEnumerable<PermissionDto>>
{
    public string? Resource { get; set; }
}
