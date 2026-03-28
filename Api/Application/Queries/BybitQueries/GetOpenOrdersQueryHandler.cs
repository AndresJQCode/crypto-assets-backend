using System.Text.Json;
using Api.Application.Dtos.Trading;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.AggregatesModel.TradingOrderAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Api.Application.Queries.BybitQueries;

internal sealed class GetOpenOrdersQueryHandler(
    IConnectorInstanceRepository connectorRepo,
    ITradingOrderRepository orderRepo,
    IBybitService bybitService,
    IEncryptionService encryptionService,
    IIdentityService identityService,
    ITenantContext tenantContext,
    ICacheService cacheService,
    ILogger<GetOpenOrdersQueryHandler> logger) : IRequestHandler<GetOpenOrdersQuery, List<TradingOrderDto>>
{
    private const int CacheTtlSeconds = 30; // 30 seconds cache to respect rate limits

    public async Task<List<TradingOrderDto>> Handle(GetOpenOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = identityService.GetCurrentUserId()
            ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
        var tenantId = tenantContext.GetCurrentTenantId()
            ?? throw new BadRequestException("No se puede obtener órdenes sin tenant en contexto.");

        // 1. Find the user's Bybit connector for this tenant
        var connector = await connectorRepo.GetByUserAndProviderAsync(
            userId,
            ConnectorProviderType.Bybit,
            cancellationToken);

        if (connector == null)
            throw new NotFoundException("No se encontró un conector Bybit para este usuario. Primero debe conectar su cuenta de Bybit.");

        if (connector.TenantId != tenantId)
            throw new UnauthorizedAccessException("El conector encontrado no pertenece al tenant actual.");

        if (!connector.IsEnabled)
            throw new BadRequestException("El conector Bybit está deshabilitado. Habilítelo antes de consultar órdenes.");

        // 2. Check cache
        var cacheKey = $"bybit_orders_{connector.Id}_{request.Symbol ?? "all"}";
        var cached = cacheService.Get<List<TradingOrderDto>>(cacheKey);
        if (cached != null)
        {
            logger.LogDebug("Cache hit for Bybit open orders. ConnectorId: {ConnectorId}, Symbol: {Symbol}",
                connector.Id, request.Symbol ?? "all");
            return cached;
        }

        logger.LogInformation("Fetching open orders from Bybit. ConnectorId: {ConnectorId}, Symbol: {Symbol}",
            connector.Id, request.Symbol ?? "all");

        // 3. Decrypt credentials
        var apiKey = await encryptionService.DecryptAsync(connector.AccessToken!);
        var config = JsonSerializer.Deserialize<BybitConfig>(connector.ConfigurationJson);
        if (config == null)
            throw new InvalidOperationException("Configuración del conector Bybit inválida.");

        var apiSecret = await encryptionService.DecryptAsync(config.ApiSecret);

        // 4. Fetch from Bybit API
        var ordersFromApi = await bybitService.GetOpenOrdersAsync(
            apiKey,
            apiSecret,
            config.IsTestnet,
            request.Symbol,
            cancellationToken);

        // 5. Sync to database (upsert: create new, update existing)
        foreach (var apiOrder in ordersFromApi)
        {
            var existing = await orderRepo.GetByExternalOrderIdAsync(
                apiOrder.OrderId,
                connector.Id,
                cancellationToken);

            if (existing == null)
            {
                // Create new trading order entity
                var newOrder = TradingOrder.CreateFromBybit(
                    connector.Id,
                    userId,
                    apiOrder.OrderId,
                    apiOrder.Symbol,
                    apiOrder.Side,
                    apiOrder.OrderType,
                    apiOrder.Status,
                    apiOrder.Qty,
                    apiOrder.Price,
                    apiOrder.CumExecQty,
                    apiOrder.AvgPrice,
                    apiOrder.Fee,
                    apiOrder.FeeCurrency,
                    apiOrder.CreatedTime,
                    apiOrder.UpdatedTime,
                    apiOrder.StopPrice,
                    apiOrder.TriggerPrice);

                await orderRepo.Create(newOrder, cancellationToken);
            }
            else
            {
                // Update existing order
                existing.UpdateFromBybit(
                    apiOrder.Status,
                    apiOrder.CumExecQty,
                    apiOrder.AvgPrice,
                    apiOrder.Fee,
                    apiOrder.FeeCurrency,
                    apiOrder.UpdatedTime);

                orderRepo.Update(existing);
            }
        }

        await orderRepo.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        logger.LogInformation("Successfully synced {Count} orders from Bybit to database. ConnectorId: {ConnectorId}",
            ordersFromApi.Count, connector.Id);

        // 6. Map to DTOs
        var result = ordersFromApi.Select(o => new TradingOrderDto
        {
            ExternalOrderId = o.OrderId,
            Symbol = o.Symbol,
            Side = o.Side,
            OrderType = o.OrderType,
            Status = o.Status,
            Quantity = o.Qty,
            Price = o.Price,
            FilledQuantity = o.CumExecQty,
            AveragePrice = o.AvgPrice,
            Fee = o.Fee,
            FeeCurrency = o.FeeCurrency,
            CreatedTime = o.CreatedTime,
            UpdatedTime = o.UpdatedTime
        }).ToList();

        // 7. Cache result
        cacheService.Set(cacheKey, result, TimeSpan.FromSeconds(CacheTtlSeconds));

        return result;
    }
}

internal sealed class BybitConfig
{
    public string ApiSecret { get; set; } = string.Empty;
    public bool IsTestnet { get; set; }
    public string Category { get; set; } = "linear";
}
