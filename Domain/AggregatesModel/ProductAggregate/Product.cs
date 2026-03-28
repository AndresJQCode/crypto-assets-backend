using Domain.SeedWork;

namespace Domain.AggregatesModel.ProductAggregate;

public class Product : Entity<Guid>, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }

    private Product() { }

    public static Product Create(Guid tenantId, string sku, string name, decimal price)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Sku = sku,
            Name = name,
            Price = price
        };
    }
}
