using Api.Application.Dtos.ConnectorInstance;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Api.Application.Commands.ConnectorInstanceCommands;

internal sealed class CreateConnectorInstanceCommandHandler(
    IConnectorDefinitionRepository definitionRepository,
    IConnectorInstanceRepository instanceRepository,
    IIdentityService identityService,
    ITenantContext tenantContext,
    IEncryptionService encryptionService) : IRequestHandler<CreateConnectorInstanceCommand, ConnectorInstanceDto>
{
    public async Task<ConnectorInstanceDto> Handle(CreateConnectorInstanceCommand request, CancellationToken cancellationToken)
    {
        Guid userId = identityService.GetCurrentUserId()
            ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
        Guid tenantId = tenantContext.GetCurrentTenantId()
            ?? throw new BadRequestException("No se puede crear un conector sin tenant en contexto.");

        var definition = await definitionRepository.GetById(request.ConnectorDefinitionId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Definición de conector no encontrada.");

        if (!definition.IsActive || definition.IsDeleted)
            throw new BadRequestException("La definición de conector no está disponible.");

        if (!Enum.TryParse<ConnectorProviderType>(definition.ProviderType, ignoreCase: true, out var providerType) ||
            providerType == ConnectorProviderType.None)
            throw new BadRequestException($"Tipo de proveedor no soportado: {definition.ProviderType}.");

        if (await instanceRepository.ExistsForUserAsync(userId, providerType, cancellationToken))
            throw new BadRequestException("Ya existe un conector de este tipo para el usuario.");

        var encryptedToken = await encryptionService.EncryptAsync(request.AccessToken);

        var entity = ConnectorInstance.CreateOAuthConnector(
            request.ConnectorDefinitionId,
            tenantId,
            userId,
            providerType,
            request.Name,
            request.ConfigurationJson,
            encryptedToken);

        await instanceRepository.Create(entity, cancellationToken);
        _ = await instanceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return MapToDto(entity);
    }

    private static ConnectorInstanceDto MapToDto(ConnectorInstance e) => new()
    {
        Id = e.Id.ToString(),
        ConnectorDefinitionId = e.ConnectorDefinitionId.ToString(),
        TenantId = e.TenantId.ToString(),
        UserId = e.UserId.ToString(),
        ProviderType = e.ProviderType.ToString(),
        CategoryType = e.CategoryType.ToString(),
        Name = e.Name,
        IsEnabled = e.IsEnabled,
        IsConfigured = e.IsConfigured,
        ConfigurationJson = e.ConfigurationJson,
        LastSyncedAt = e.LastSyncedAt,
        Status = e.GetStatus().ToString()
    };
}
