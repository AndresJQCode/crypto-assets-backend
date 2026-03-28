using Api.Application.Dtos.ConnectorInstance;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.Exceptions;
using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

internal sealed class SetConnectorInstanceEnabledCommandHandler(
    IConnectorInstanceRepository repository,
    ITenantContext tenantContext) : IRequestHandler<SetConnectorInstanceEnabledCommand, ConnectorInstanceDto>
{
    public async Task<ConnectorInstanceDto> Handle(SetConnectorInstanceEnabledCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.Id, tracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Conector no encontrado.");

        if (entity.IsDeleted)
            throw new NotFoundException("Conector no encontrado.");

        var tenantId = tenantContext.GetCurrentTenantId();
        if (tenantId.HasValue && entity.TenantId != tenantId.Value)
            throw new BadRequestException("No tiene permiso para modificar este conector.");

        if (request.IsEnabled)
            entity.Enable();
        else
            entity.Disable();

        _ = repository.Update(entity);
        _ = await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new ConnectorInstanceDto
        {
            Id = entity.Id.ToString(),
            ConnectorDefinitionId = entity.ConnectorDefinitionId.ToString(),
            TenantId = entity.TenantId.ToString(),
            UserId = entity.UserId.ToString(),
            ProviderType = entity.ProviderType.ToString(),
            CategoryType = entity.CategoryType.ToString(),
            Name = entity.Name,
            IsEnabled = entity.IsEnabled,
            IsConfigured = entity.IsConfigured,
            ConfigurationJson = entity.ConfigurationJson,
            LastSyncedAt = entity.LastSyncedAt,
            Status = entity.GetStatus().ToString()
        };
    }
}
