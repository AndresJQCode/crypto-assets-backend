using Domain.AggregatesModel.AuditAggregate;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

/// <summary>
/// Persiste registros de auditoría en un scope/transacción independiente del pipeline actual,
/// de modo que el log se guarde aunque el handler lance una excepción (p. ej. login fallido).
/// Rellena la IP del cliente desde el contexto HTTP cuando no viene en el registro.
/// </summary>
public sealed class OutOfTransactionAuditLogWriter(
    IServiceScopeFactory scopeFactory,
    IClientIpProvider clientIpProvider) : IOutOfTransactionAuditLogWriter
{
    public async Task SaveAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        auditLog.SetClientIpIfEmpty(clientIpProvider.GetClientIp());

        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
        await repo.Create(auditLog, cancellationToken);
        await repo.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
