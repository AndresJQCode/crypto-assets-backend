using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed class LogoutCommand(string id) : IRequest
{
    public string Id { get; set; } = id;
}
