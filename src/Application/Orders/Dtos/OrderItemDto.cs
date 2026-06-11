namespace Application.Orders.Dtos;

/// <summary>
/// Data transfer object representing a single line item within an order.
/// </summary>
public sealed record OrderItemDto
{
    /// <summary>Gets the line item's unique identifier.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the identifier of the product this line item represents.</summary>
    public required Guid ProductId { get; init; }

    /// <summary>Gets a snapshot of the product's name at the time it was added.</summary>
    public required string ProductName { get; init; }

    /// <summary>Gets the unit price of the product at the time it was added.</summary>
    public required decimal UnitPrice { get; init; }

    /// <summary>Gets the ISO 4217 currency code of <see cref="UnitPrice"/> and <see cref="Subtotal"/>.</summary>
    public required string Currency { get; init; }

    /// <summary>Gets the quantity of the product ordered.</summary>
    public required int Quantity { get; init; }

    /// <summary>Gets the subtotal for this line item (<see cref="UnitPrice"/> multiplied by <see cref="Quantity"/>).</summary>
    public required decimal Subtotal { get; init; }
}
