using System.Text.Json;
using Domain.AggregatesModel.ConnectorInstanceAggregate;
using Domain.AggregatesModel.ConnectorInstanceAggregate.Configurations;
using Domain.AggregatesModel.TradingOrderAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Api.Application.Commands.BybitCommands;

/// <summary>
/// Handler for synchronizing full trading history from Bybit
/// </summary>
public class SyncBybitHistoryCommandHandler(
    IConnectorInstanceRepository connectorRepo,
    ITradingOrderRepository orderRepo,
    IBybitService bybitService,
    ILogger<SyncBybitHistoryCommandHandler> logger)
    : IRequestHandler<SyncBybitHistoryCommand, SyncBybitHistoryResult>
{
    private const int MaxOrdersPerCall = 50; // Bybit API limit
    private const int RateLimitDelayMs = 200; // Delay between API calls to avoid rate limit

    public async Task<SyncBybitHistoryResult> Handle(SyncBybitHistoryCommand request, CancellationToken cancellationToken)
    {
        var syncStartTime = DateTime.UtcNow;
        var errors = new List<string>();
        var totalFetched = 0;
        var newOrders = 0;
        var updatedOrders = 0;
        var totalApiCalls = 0;

        try
        {
            // 1. Get connector instance with credentials
            var connector = await connectorRepo.GetById(request.ConnectorInstanceId, cancellationToken: cancellationToken)
                ?? throw new NotFoundException($"Connector instance {request.ConnectorInstanceId} not found");

            if (connector.ProviderType != ConnectorProviderType.Bybit)
                throw new BadRequestException("Connector is not a Bybit connector");

            if (!connector.IsEnabled)
                throw new BadRequestException("Connector is disabled");

            var config = JsonSerializer.Deserialize<BybitConfiguration>(connector.ConfigurationJson)
                ?? throw new BadRequestException("Invalid Bybit configuration");

            logger.LogInformation(
                "Starting Bybit history sync for connector {ConnectorId}. StartDate: {StartDate}, EndDate: {EndDate}, Symbol: {Symbol}",
                request.ConnectorInstanceId, request.StartDate, request.EndDate, request.Symbol ?? "all");

            // 2. Determine date range
            var startDate = request.StartDate ?? DateTime.UtcNow.AddYears(-2); // Default: 2 years back
            var endDate = request.EndDate ?? DateTime.UtcNow;

            // 3. Sync in 7-day chunks to avoid overwhelming the API
            var currentStart = startDate;
            var chunkSize = TimeSpan.FromDays(7);

            while (currentStart < endDate)
            {
                var currentEnd = currentStart.Add(chunkSize);
                if (currentEnd > endDate)
                    currentEnd = endDate;

                try
                {
                    logger.LogInformation(
                        "Syncing chunk: {StartDate} to {EndDate}",
                        currentStart, currentEnd);

                    var orders = await bybitService.GetOrderHistoryAsync(
                        config.ApiKey,
                        config.ApiSecret,
                        config.IsTestnet,
                        currentStart,
                        currentEnd,
                        request.Symbol,
                        MaxOrdersPerCall,
                        cancellationToken);

                    totalApiCalls++;
                    totalFetched += orders.Count;

                    logger.LogInformation("Fetched {Count} orders for chunk", orders.Count);

                    // 4. Store/update orders in database
                    foreach (var orderDto in orders)
                    {
                        try
                        {
                            var existingOrder = await orderRepo.GetByExternalOrderIdAsync(
                                orderDto.OrderId,
                                request.ConnectorInstanceId,
                                cancellationToken);

                            if (existingOrder == null)
                            {
                                // Create new order
                                var newOrder = TradingOrder.CreateFromBybit(
                                    connectorInstanceId: request.ConnectorInstanceId,
                                    userId: Guid.Empty, // Will be set from connector context
                                    externalOrderId: orderDto.OrderId,
                                    symbol: orderDto.Symbol,
                                    side: orderDto.Side,
                                    orderType: orderDto.OrderType,
                                    status: orderDto.Status,
                                    quantity: orderDto.Qty,
                                    price: orderDto.Price,
                                    filledQuantity: orderDto.CumExecQty,
                                    averagePrice: orderDto.AvgPrice,
                                    fee: orderDto.Fee,
                                    feeCurrency: orderDto.FeeCurrency,
                                    createdTime: orderDto.CreatedTime,
                                    updatedTime: orderDto.UpdatedTime,
                                    stopPrice: orderDto.StopPrice,
                                    triggerPrice: orderDto.TriggerPrice
                                );

                                await orderRepo.Create(newOrder, cancellationToken);
                                newOrders++;
                            }
                            else
                            {
                                // Update existing order
                                existingOrder.UpdateFromBybit(
                                    status: orderDto.Status,
                                    filledQuantity: orderDto.CumExecQty,
                                    averagePrice: orderDto.AvgPrice,
                                    fee: orderDto.Fee,
                                    feeCurrency: orderDto.FeeCurrency,
                                    updatedTime: orderDto.UpdatedTime
                                );

                                orderRepo.Update(existingOrder);
                                updatedOrders++;
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMsg = $"Error processing order {orderDto.OrderId}: {ex.Message}";
                            logger.LogError(ex, "Error processing order {OrderId}: {ErrorMessage}", orderDto.OrderId, ex.Message);
                            errors.Add(errorMsg);
                        }
                    }

                    // Save changes for this chunk
                    await orderRepo.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                    // Rate limit protection
                    await Task.Delay(RateLimitDelayMs, cancellationToken);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Error syncing chunk {currentStart} to {currentEnd}: {ex.Message}";
                    logger.LogError(ex, "Error syncing chunk {StartDate} to {EndDate}: {ErrorMessage}", currentStart, currentEnd, ex.Message);
                    errors.Add(errorMsg);
                }

                currentStart = currentEnd;
            }

            var syncEndTime = DateTime.UtcNow;

            logger.LogInformation(
                "Bybit history sync completed. Total fetched: {TotalFetched}, New: {New}, Updated: {Updated}, API calls: {ApiCalls}, Errors: {Errors}",
                totalFetched, newOrders, updatedOrders, totalApiCalls, errors.Count);

            return new SyncBybitHistoryResult
            {
                TotalOrdersFetched = totalFetched,
                NewOrdersStored = newOrders,
                UpdatedOrders = updatedOrders,
                TotalApiCalls = totalApiCalls,
                SyncStartTime = syncStartTime,
                SyncEndTime = syncEndTime,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during Bybit history sync");
            throw new DomainException($"Error al sincronizar historial de Bybit: {ex.Message}", ex);
        }
    }
}
