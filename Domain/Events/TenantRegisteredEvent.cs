using MediatR;

namespace Domain.Events;

/// <summary>
/// Domain event raised when a new tenant is registered in the system
/// </summary>
public class TenantRegisteredEvent : INotification
{
    public Guid TenantId { get; }
    public string TenantName { get; }
    public string TenantSlug { get; }
    public string AdminEmail { get; }
    public string AdminName { get; }
    public DateTimeOffset RegisteredAt { get; }

    public TenantRegisteredEvent(
        Guid tenantId,
        string tenantName,
        string tenantSlug,
        string adminEmail,
        string adminName)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        TenantSlug = tenantSlug;
        AdminEmail = adminEmail;
        AdminName = adminName;
        RegisteredAt = DateTimeOffset.UtcNow;
    }
}
