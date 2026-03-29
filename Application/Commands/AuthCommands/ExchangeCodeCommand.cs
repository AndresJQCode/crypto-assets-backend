using Application.Dtos.Auth;
using MediatR;

namespace Application.Commands.AuthCommands;

internal sealed class ExchangeCodeCommand : IRequest<ExchangeCodeResponseDto>
{
    public required string Code { get; set; }
    public required string Provider { get; set; }
}
