using Api.Application.Dtos.Permission;
using MediatR;

namespace Api.Application.Queries.PermissionQueries;

internal sealed class GetAllPermissionsSimpleQuery : IRequest<IEnumerable<PermissionDto>>
{
    public string? Resource { get; set; }
}
