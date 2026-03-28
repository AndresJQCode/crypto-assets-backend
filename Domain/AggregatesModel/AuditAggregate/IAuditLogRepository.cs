using Domain.SeedWork;

namespace Domain.AggregatesModel.AuditAggregate;

/// <summary>
/// Repositorio para registros de auditoría.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
}
