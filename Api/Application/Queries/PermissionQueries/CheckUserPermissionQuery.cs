using MediatR;

namespace Api.Application.Queries.PermissionQueries;

internal sealed record CheckUserPermissionQuery(
    Guid UserId,
    string Resource,
    string Action
) : IRequest<bool>;
