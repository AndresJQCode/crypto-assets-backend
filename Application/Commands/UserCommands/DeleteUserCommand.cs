using MediatR;

namespace Application.Commands.UserCommands;

internal sealed record DeleteUserCommand(Guid Id, string? Reason = null) : IRequest<bool>;
