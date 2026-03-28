using Domain.SeedWork;
using Microsoft.AspNetCore.Identity;

namespace Domain.AggregatesModel.UserAggregate;

public class User : IdentityUser<Guid>, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Número de teléfono para WhatsApp (ej. +573001234567). Usado para contacto y notificaciones.</summary>
    public string? WhatsAppNumber { get; set; }
    public bool IsActive { get; private set; } = true;
    /// <summary>
    /// Null para usuarios de plataforma (SuperAdmin) que pueden ver todos los tenants.
    /// </summary>
    public Guid? TenantId { get; set; }
    public ICollection<UserRole> UserRoles { get; init; } = [];

    public User()
    {
        Id = Guid.CreateVersion7();
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
