using Application.Dtos.Permission;
using MediatR;

namespace Application.Queries.PermissionQueries;

internal sealed record GetPermissionByIdQuery(Guid Id) : IRequest<PermissionDto?>;
