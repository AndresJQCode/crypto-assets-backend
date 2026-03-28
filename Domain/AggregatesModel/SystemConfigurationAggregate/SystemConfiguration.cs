using Domain.SeedWork;

namespace Domain.AggregatesModel.SystemConfigurationAggregate;

/// <summary>
/// Stores system-wide operational configuration flags.
/// Used for feature flags, operational controls, and deployment management.
/// </summary>
public class SystemConfiguration : Entity<Guid>, IAggregateRoot
{
    public string Key { get; private set; } = default!;
    public string Value { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private SystemConfiguration() { }

    public static SystemConfiguration Create(
        string key,
        string value,
        string? description,
        string createdBy)
    {
        return new SystemConfiguration
        {
            Id = Guid.NewGuid(),
            Key = key,
            Value = value,
            Description = description,
            IsActive = true,
            CreatedBy = Guid.TryParse(createdBy, out var userId) ? userId : null,
            CreatedOn = DateTimeOffset.UtcNow,
            LastModifiedOn = DateTimeOffset.UtcNow,
            LastModifiedByName = createdBy
        };
    }

    public void UpdateValue(string newValue, string modifiedBy)
    {
        Value = newValue;
        LastModifiedOn = DateTimeOffset.UtcNow;
        LastModifiedByName = modifiedBy;
    }

    public void UpdateDescription(string? newDescription, string modifiedBy)
    {
        Description = newDescription;
        LastModifiedOn = DateTimeOffset.UtcNow;
        LastModifiedByName = modifiedBy;
    }

    public void Activate(string modifiedBy)
    {
        IsActive = true;
        LastModifiedOn = DateTimeOffset.UtcNow;
        LastModifiedByName = modifiedBy;
    }

    public void Deactivate(string modifiedBy)
    {
        IsActive = false;
        LastModifiedOn = DateTimeOffset.UtcNow;
        LastModifiedByName = modifiedBy;
    }

    public bool GetBoolValue() => Value.Equals("true", StringComparison.OrdinalIgnoreCase);

    public int GetIntValue() => int.TryParse(Value, out var result) ? result : 0;
}
