using Api.Application.Dtos.ConnectorInstance;
using MediatR;

namespace Api.Application.Commands.BybitCommands;

internal sealed class CreateBybitConnectorCommand : IRequest<ConnectorInstanceDto>
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool IsTestnet { get; set; }
}
