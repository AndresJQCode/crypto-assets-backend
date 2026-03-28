namespace Domain.SeedWork;

/// <summary>
/// Interface para el servicio de auditoría que registra las operaciones de eliminación
/// </summary>
public interface IAuditTrail
{
    /// <summary>
    /// Registra la eliminación de una entidad
    /// </summary>
    /// <param name="entityType">Tipo de entidad</param>
    /// <param name="entityId">ID de la entidad</param>
    /// <param name="deletedBy">Usuario que eliminó</param>
    /// <param name="deletedByName">Nombre del usuario que eliminó</param>
    /// <param name="reason">Razón de la eliminación</param>
    /// <param name="entitySnapshot">Snapshot de la entidad antes de eliminar (se serializa a JSON)</param>
    Task LogDeletionAsync(
        string entityType,
        Guid entityId,
        Guid? deletedBy,
        string? deletedByName,
        string? reason = null,
        object? entitySnapshot = null);
}
