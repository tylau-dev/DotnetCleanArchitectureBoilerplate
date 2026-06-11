namespace Application.Orders.Dtos;

/// <summary>
/// Data transfer object representing a customer's order, including its line items, shipping
/// address, and lifecycle status.
/// </summary>
public sealed record OrderDto
{
    /// <summary>Gets the order's unique identifier.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the identifier of the customer who placed the order.</summary>
    public required Guid CustomerId { get; init; }

    /// <summary>Gets the current lifecycle status of the order.</summary>
    public required OrderStatusDto Status { get; init; }

    /// <summary>Gets the address the order will be shipped to.</summary>
    public required AddressDto ShippingAddress { get; init; }

    /// <summary>Gets the line items that make up this order.</summary>
    public required IReadOnlyCollection<OrderItemDto> Items { get; init; }

    /// <summary>Gets the total amount of the order.</summary>
    public required decimal TotalAmount { get; init; }

    /// <summary>Gets the ISO 4217 currency code of <see cref="TotalAmount"/>.</summary>
    public required string Currency { get; init; }

    /// <summary>Gets the UTC timestamp at which the order was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }
}
