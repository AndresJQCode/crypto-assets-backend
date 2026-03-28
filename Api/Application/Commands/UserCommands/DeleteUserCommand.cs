using MediatR;

namespace Api.Application.Commands.UserCommands;

internal sealed record DeleteUserCommand(Guid Id, string? Reason = null) : IRequest<bool>;
