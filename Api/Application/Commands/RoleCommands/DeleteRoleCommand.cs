using MediatR;

namespace Api.Application.Commands.RoleCommands;

internal sealed record DeleteRoleCommand(Guid Id) : IRequest<bool>;
