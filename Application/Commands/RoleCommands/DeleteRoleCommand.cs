using MediatR;

namespace Application.Commands.RoleCommands;

internal sealed record DeleteRoleCommand(Guid Id) : IRequest<bool>;
