using Api.Application.Dtos.Auth;
using MediatR;

namespace Api.Application.Commands.AuthCommands;

internal sealed class ExchangeCodeCommand : IRequest<ExchangeCodeResponseDto>
{
    public required string Code { get; set; }
    public required string Provider { get; set; }
    /// <summary>State opcional: string URL-encoded que al decodificar es un JSON de <see cref="TenantRequest"/>.</summary>
    public string? State { get; set; }
}
