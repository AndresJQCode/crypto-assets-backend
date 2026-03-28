using System.Text.Json;
using Domain.AggregatesModel.AuditAggregate;
using Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Servicio de auditoría que registra las operaciones de eliminación
/// </summary>
public class AuditTrailService : IAuditTrail
{
    private readonly ILogger<AuditTrailService> _logger;
    private readonly ApiContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditTrailService(ILogger<AuditTrailService> logger, ApiContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Registra la eliminación de una entidad con snapshot de sus datos
    /// </summary>
    public async Task LogDeletionAsync(
        string entityType,
        Guid entityId,
        Guid? deletedBy,
        string? deletedByName,
        string? reason = null,
        object? entitySnapshot = null)
    {
        try
        {
            string? snapshotJson = entitySnapshot is not null
                ? JsonSerializer.Serialize(entitySnapshot, JsonOptions)
                : null;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Entidad {EntityType} con ID {EntityId} eliminada por {DeletedByName} (ID: {DeletedBy}). Razón: {Reason}. Snapshot: {Snapshot}",
                    entityType,
                    entityId,
                    deletedByName ?? "Usuario desconocido",
                    deletedBy,
                    reason ?? "No especificada",
                    snapshotJson ?? "N/A");
            }

            var auditLog = AuditLog.CreateDeletionLog(
                entityType,
                entityId,
                deletedBy,
                deletedByName,
                reason,
                snapshotJson);

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar eliminación de entidad {EntityType} con ID {EntityId}", entityType, entityId);
        }
    }
}
