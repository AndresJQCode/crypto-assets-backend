using Api.Application.Dtos.Permission;
using MediatR;

namespace Api.Application.Queries.PermissionQueries;

internal sealed record GetPermissionByIdQuery(Guid Id) : IRequest<PermissionDto?>;
