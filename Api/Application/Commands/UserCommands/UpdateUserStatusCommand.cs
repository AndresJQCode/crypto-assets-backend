using MediatR;

namespace Api.Application.Commands.UserCommands;

internal sealed record UpdateUserStatusCommand(Guid Id, bool IsActive) : IRequest<bool>;
