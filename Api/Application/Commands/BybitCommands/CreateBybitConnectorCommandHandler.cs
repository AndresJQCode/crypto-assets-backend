using System.Text.Json;
using Api.Application.Dtos.ConnectorInstance;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.ConnectorDefinitionAggregate;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Api.Application.Commands.BybitCommands;

internal sealed class CreateBybitConnectorCommandHandler(
    IConnectorDefinitionRepository definitionRepository,
    IConnectorInstanceRepository instanceRepository,
    IBybitService bybitService,
    IEncryptionService encryptionService,
    IIdentityService identityService,
    ITenantContext tenantContext) : IRequestHandler<CreateBybitConnectorCommand, ConnectorInstanceDto>
{
    public async Task<ConnectorInstanceDto> Handle(CreateBybitConnectorCommand request, CancellationToken cancellationToken)
    {
        var userId = identityService.GetCurrentUserId()
            ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
        var tenantId = tenantContext.GetCurrentTenantId()
            ?? throw new BadRequestException("No se puede crear un conector sin tenant en contexto.");

        // Validate Bybit credentials before creating connector
        var isValid = await bybitService.ValidateCredentialsAsync(
            request.ApiKey,
            request.ApiSecret,
            request.IsTestnet,
            cancellationToken);

        if (!isValid)
            throw new BadRequestException("Las credenciales de Bybit no son válidas. Verifique su API key y secret.");

        // Get Bybit connector definition (should be seeded)
        var definition = await definitionRepository.GetByProviderTypeAsync("Bybit", cancellationToken)
            ?? throw new NotFoundException("Definición de conector Bybit no encontrada. Ejecute el seed de datos.");

        if (!definition.IsActive || definition.IsDeleted)
            throw new BadRequestException("La definición de conector Bybit no está disponible.");

        // Check if user already has a Bybit connector
        if (await instanceRepository.ExistsForUserAsync(userId, ConnectorProviderType.Bybit, cancellationToken))
            throw new BadRequestException("Ya existe un conector Bybit para este usuario. Elimine el existente antes de crear uno nuevo.");

        // Encrypt API key and secret
        var encryptedApiKey = await encryptionService.EncryptAsync(request.ApiKey);
        var encryptedApiSecret = await encryptionService.EncryptAsync(request.ApiSecret);

        // Store secret and config in ConfigurationJson
        var configJson = JsonSerializer.Serialize(new
        {
            apiSecret = encryptedApiSecret,
            isTestnet = request.IsTestnet,
            category = "linear" // USDT perpetual futures
        });

        // Create connector instance using API key pattern
        var entity = ConnectorInstance.CreateApiKeyConnector(
            definition.Id,
            tenantId,
            userId,
            ConnectorProviderType.Bybit,
            request.Name,
            configJson,
            encryptedApiKey); // Stored in AccessToken field

        await instanceRepository.Create(entity, cancellationToken);
        await instanceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

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
