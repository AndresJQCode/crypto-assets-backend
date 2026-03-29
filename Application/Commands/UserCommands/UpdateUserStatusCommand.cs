using MediatR;

namespace Application.Commands.UserCommands;

internal sealed record UpdateUserStatusCommand(Guid Id, bool IsActive) : IRequest<bool>;
