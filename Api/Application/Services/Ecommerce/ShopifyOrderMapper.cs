using System.Text.Json;
using Api.Application.Dtos.Shopify;
using Domain.AggregatesModel.OrderAggregate;
using Domain.Exceptions;

namespace Api.Application.Services.Ecommerce;

public class ShopifyOrderMapper : IOrderMapper
{
    public Order MapToOrder(string eventPayload, Guid tenantId, Guid customerId, DateTime eventTimestamp)
    {
        var dto = JsonSerializer.Deserialize<ShopifyOrderDto>(eventPayload)
            ?? throw new DomainException("Invalid Shopify payload");

        var shippingAddress = ShippingAddress.Create(
            dto.ShippingAddress.FirstName,
            dto.ShippingAddress.LastName,
            dto.ShippingAddress.Address1,
            dto.ShippingAddress.City,
            dto.ShippingAddress.Country,
            dto.ShippingAddress.Address2,
            dto.ShippingAddress.Province,
            dto.ShippingAddress.Zip,
            dto.ShippingAddress.Phone
        );

        var billingAddress = BillingAddress.Create(
            dto.BillingAddress.FirstName,
            dto.BillingAddress.LastName,
            dto.BillingAddress.Address1,
            dto.BillingAddress.City,
            dto.BillingAddress.Country,
            dto.BillingAddress.Address2,
            dto.BillingAddress.Province,
            dto.BillingAddress.Zip,
            dto.BillingAddress.Phone
        );

        var order = Order.CreateFromEcommerce(
            tenantId,
            customerId,
            dto.OrderNumber,
            dto.Id.ToString(),
            EcommercePlatform.Shopify,
            dto.TotalPrice,
            dto.Currency,
            dto.CreatedAt,
            shippingAddress,
            billingAddress
        );

        foreach (var item in dto.LineItems)
        {
            var orderItem = OrderItem.Create(
                item.Sku,
                item.Title,
                item.Quantity,
                item.Price,
                item.VariantTitle,
                productId: null,
                imageUrl: null
            );
            order.AddItem(orderItem);
        }

        order.UpdateFromEcommerce(
            status: MapOrderStatus(dto.FulfillmentStatus),
            paymentStatus: MapPaymentStatus(dto.FinancialStatus),
            fulfillmentStatus: MapFulfillmentStatus(dto.FulfillmentStatus),
            subtotal: dto.Subtotal,
            tax: dto.TotalTax,
            shippingCost: dto.TotalShipping,
            discount: dto.TotalDiscounts,
            total: dto.TotalPrice,
            eventTimestamp: eventTimestamp,
            platformMetadata: eventPayload
        );

        return order;
    }

    public void UpdateOrder(Order order, string eventPayload, DateTime eventTimestamp)
    {
        var dto = JsonSerializer.Deserialize<ShopifyOrderDto>(eventPayload)
            ?? throw new DomainException("Invalid Shopify payload");

        order.UpdateFromEcommerce(
            status: MapOrderStatus(dto.FulfillmentStatus),
            paymentStatus: MapPaymentStatus(dto.FinancialStatus),
            fulfillmentStatus: MapFulfillmentStatus(dto.FulfillmentStatus),
            subtotal: dto.Subtotal,
            tax: dto.TotalTax,
            shippingCost: dto.TotalShipping,
            discount: dto.TotalDiscounts,
            total: dto.TotalPrice,
            eventTimestamp: eventTimestamp,
            platformMetadata: eventPayload
        );
    }

    private OrderStatus MapOrderStatus(string? fulfillmentStatus) => fulfillmentStatus switch
    {
        "fulfilled" => OrderStatus.Completed,
        "partial" => OrderStatus.Processing,
        _ => OrderStatus.Pending
    };

    private PaymentStatus MapPaymentStatus(string financialStatus) => financialStatus switch
    {
        "paid" => PaymentStatus.Paid,
        "authorized" => PaymentStatus.Authorized,
        "partially_refunded" => PaymentStatus.PartiallyRefunded,
        "refunded" => PaymentStatus.Refunded,
        "voided" => PaymentStatus.Voided,
        _ => PaymentStatus.Pending
    };

    private FulfillmentStatus MapFulfillmentStatus(string? fulfillmentStatus) => fulfillmentStatus switch
    {
        "fulfilled" => FulfillmentStatus.Fulfilled,
        "partial" => FulfillmentStatus.PartiallyFulfilled,
        "restocked" => FulfillmentStatus.Restocked,
        _ => FulfillmentStatus.Unfulfilled
    };
}
