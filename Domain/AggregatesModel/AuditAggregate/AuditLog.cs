using Domain.SeedWork;

namespace Domain.AggregatesModel.AuditAggregate;

/// <summary>
/// Entidad que representa un registro de auditoría en el sistema
/// </summary>
public class AuditLog : Entity<Guid>, IAggregateRoot
{
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty; // DELETE, RESTORE, HARD_DELETE
    public Guid? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string? AdditionalData { get; private set; } // JSON con datos adicionales
    public string? Ip { get; private set; }

    /// <summary>
    /// Asigna la IP del cliente cuando no fue proporcionada en el constructor.
    /// Usado por infraestructura (p. ej. escritor de auditoría fuera de transacción) para rellenar desde el contexto HTTP.
    /// </summary>
    public void SetClientIpIfEmpty(string? ip)
    {
        if (string.IsNullOrEmpty(Ip))
            Ip = ip;
    }

    private AuditLog() { } // Para EF Core

    public AuditLog(
        string entityType,
        Guid entityId,
        string action,
        Guid? userId = null,
        string? userName = null,
        string? reason = null,
        string? additionalData = null,
        string? ip = null)
    {
        Id = Guid.CreateVersion7();
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        EntityId = entityId;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        UserId = userId;
        UserName = userName;
        Reason = reason;
        Timestamp = DateTimeOffset.UtcNow;
        AdditionalData = additionalData;
        Ip = ip;

        // Inicializar propiedades heredadas de Entity
        CreatedOn = DateTimeOffset.UtcNow;
        CreatedBy = userId;
    }

    /// <summary>
    /// Crea un registro de auditoría para eliminación
    /// </summary>
    /// <param name="entityType">Tipo de entidad eliminada</param>
    /// <param name="entityId">ID de la entidad</param>
    /// <param name="userId">ID del usuario que eliminó</param>
    /// <param name="userName">Nombre del usuario que eliminó</param>
    /// <param name="reason">Razón de la eliminación</param>
    /// <param name="additionalData">JSON con snapshot de la entidad u otros datos relevantes</param>
    /// <param name="ip">Dirección IP del cliente (opcional)</param>
    public static AuditLog CreateDeletionLog(
        string entityType,
        Guid entityId,
        Guid? userId = null,
        string? userName = null,
        string? reason = null,
        string? additionalData = null,
        string? ip = null)
    {
        return new AuditLog(entityType, entityId, "DELETE", userId, userName, reason, additionalData, ip);
    }
}
