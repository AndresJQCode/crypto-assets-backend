using Domain.AggregatesModel.AuditAggregate;

namespace Domain.Interfaces;

/// <summary>
/// Persiste un registro de auditoría en una transacción independiente del pipeline actual.
/// Útil cuando se debe guardar el audit log aunque el handler lance una excepción
/// (p. ej. login fallido), ya que la transacción del TransactionBehavior haría rollback.
/// </summary>
public interface IOutOfTransactionAuditLogWriter
{
    /// <summary>
    /// Guarda el registro de auditoría en un scope/transacción independiente.
    /// </summary>
    Task SaveAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
