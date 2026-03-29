using Api.Application.Dtos.Auth;
using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed class ExchangeCodeCommand : IRequest<ExchangeCodeResponseDto>
{
    public required string Code { get; set; }
    public required string Provider { get; set; }
}
