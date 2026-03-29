using MediatR;

namespace Application.Queries.PermissionQueries;

internal sealed record CheckUserPermissionQuery(
    Guid UserId,
    string Resource,
    string Action
) : IRequest<bool>;
