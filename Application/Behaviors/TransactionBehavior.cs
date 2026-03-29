namespace Application.Behaviors;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Extensions;
using global::Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

internal sealed class TransactionBehavior<TRequest, TResponse>(
    ApiContext dbContext,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = default(TResponse);
        var typeName = request.GetGenericTypeName();

        try
        {
            if (dbContext.HasActiveTransaction)
            {
                return await next(cancellationToken);
            }

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                Guid transactionId;

                await using var transaction = await dbContext.BeginTransactionAsync();
                if (transaction != null)
                {
                    using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("TransactionContext", transaction.TransactionId) }))
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})", transaction.TransactionId, typeName, request);
                        }

                        response = await next();

                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("Commit transaction {TransactionId} for {CommandName}", transaction.TransactionId, typeName);
                        }

                        await dbContext.CommitTransactionAsync(transaction);

                        transactionId = transaction.TransactionId;
                    }
                }

            });

            return response!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error Handling transaction for {CommandName} ({@Command})", typeName, request);

            throw;
        }
    }
}
