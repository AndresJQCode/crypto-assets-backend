using Domain.SeedWork;

namespace Domain.AggregatesModel.CustomerAggregate;

public class Customer : Entity<Guid>, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string? Phone { get; private set; }

    private Customer() { }

    public static Customer Create(Guid tenantId, string email, string firstName, string lastName, string? phone = null)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone
        };
    }
}
