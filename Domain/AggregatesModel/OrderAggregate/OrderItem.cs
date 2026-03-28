using Domain.SeedWork;

namespace Domain.AggregatesModel.OrderAggregate;

public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid? ProductId { get; private set; }
    public string ExternalProductId { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public string? VariantName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => Quantity * UnitPrice;

#pragma warning disable CA1056 // URI properties should not be strings
    public string? ImageUrl { get; private set; }
#pragma warning restore CA1056

    private OrderItem() { }

#pragma warning disable CA1054 // URI parameters should not be strings
    public static OrderItem Create(
        string externalProductId,
        string productName,
        int quantity,
        decimal unitPrice,
        string? variantName = null,
        Guid? productId = null,
        string? imageUrl = null)
#pragma warning restore CA1054
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            ExternalProductId = externalProductId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            VariantName = variantName,
            ProductId = productId,
            ImageUrl = imageUrl
        };
    }
}
