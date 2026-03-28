using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

/// <summary>
/// Validates that the connector connection is active (e.g. token still valid).
/// Returns current status; future: call provider API to validate.
/// </summary>
internal sealed class ValidateConnectorInstanceCommand(Guid id) : IRequest<ValidateConnectorInstanceResult>
{
    public Guid Id { get; } = id;
}

internal sealed class ValidateConnectorInstanceResult
{
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}
