using Domain.Events;
using Domain.SeedWork;

namespace Domain.AggregatesModel.TenantAggregate;

public class Tenant : Entity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string CountryName { get; private set; } = string.Empty;
    public string CountryPhoneCode { get; private set; } = string.Empty;
    public string WhatsAppNumber { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Tenant() { }

    public Tenant(string name, string slug, Guid? createdBy = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
#pragma warning disable CA1308 // Slug para URLs se normaliza en minúsculas
        Slug = (slug ?? name).ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal);
#pragma warning restore CA1308
        IsActive = true;
        CreatedOn = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Factory method for creating a tenant during registration process
    /// Raises TenantRegisteredEvent for notifications
    /// </summary>
    public static Tenant CreateForRegistration(
        string tenantName,
        string tenantSlug,
        string adminEmail,
        string adminName,
        Guid? createdBy = null)
    {
        var tenant = new Tenant(tenantName, tenantSlug, createdBy);

        // Raise domain event for notifications
        tenant.AddDomainEvent(new TenantRegisteredEvent(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            adminEmail,
            adminName));

        return tenant;
    }

    public void Update(string name, string slug, Guid? modifiedBy = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
#pragma warning disable CA1308 // Slug para URLs se normaliza en minúsculas
        Slug = (slug ?? name).ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal);
#pragma warning restore CA1308
        LastModifiedOn = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void SetContactInfo(string countryName, string countryPhoneCode, string whatsAppNumber)
    {
        CountryName = countryName ?? string.Empty;
        CountryPhoneCode = countryPhoneCode ?? string.Empty;
        WhatsAppNumber = whatsAppNumber ?? string.Empty;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
