namespace Api.Application.Dtos.Shopify;

public record ShopifyOrderDto(
    long Id,
    string OrderNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string FinancialStatus,
    string? FulfillmentStatus,
    decimal TotalPrice,
    decimal Subtotal,
    decimal TotalTax,
    decimal TotalShipping,
    decimal TotalDiscounts,
    string Currency,
    ShopifyCustomerDto Customer,
    List<ShopifyLineItemDto> LineItems,
    ShopifyAddressDto ShippingAddress,
    ShopifyAddressDto BillingAddress,
    DateTime? CancelledAt,
    string? CancelReason
);

public record ShopifyLineItemDto(
    long Id,
    long ProductId,
    long? VariantId,
    string Title,
    string? VariantTitle,
    int Quantity,
    decimal Price,
    string Sku
);

public record ShopifyCustomerDto(
    long Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone
);

public record ShopifyAddressDto(
    string FirstName,
    string LastName,
    string Address1,
    string? Address2,
    string City,
    string? Province,
    string Country,
    string Zip,
    string? Phone
);
