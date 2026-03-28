using Api.Infrastructure.Services;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

internal sealed class ValidateConnectorInstanceCommandHandler(
    IConnectorInstanceRepository repository,
    ITenantContext tenantContext) : IRequestHandler<ValidateConnectorInstanceCommand, ValidateConnectorInstanceResult>
{
    public async Task<ValidateConnectorInstanceResult> Handle(ValidateConnectorInstanceCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.Id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Conector no encontrado.");

        if (entity.IsDeleted)
            throw new NotFoundException("Conector no encontrado.");

        var tenantId = tenantContext.GetCurrentTenantId();
        if (tenantId.HasValue && entity.TenantId != tenantId.Value)
            throw new BadRequestException("No tiene permiso para validar este conector.");

        var status = entity.GetStatus();
        var isValid = status == ConnectorStatus.Active;

        return new ValidateConnectorInstanceResult
        {
            IsValid = isValid,
            Status = status.ToString(),
            Message = isValid ? "Conexión activa." : GetMessageForStatus(status)
        };
    }

    private static string GetMessageForStatus(ConnectorStatus status) => status switch
    {
        ConnectorStatus.NotConfigured => "El conector no está configurado.",
        ConnectorStatus.Disabled => "El conector está deshabilitado.",
        ConnectorStatus.Error => "Error de conexión (por ejemplo, token expirado). Considere reautorizar.",
        _ => "Estado desconocido."
    };
}
