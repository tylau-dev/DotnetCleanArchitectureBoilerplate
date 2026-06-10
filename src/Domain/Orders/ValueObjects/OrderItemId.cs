namespace Domain.Orders.ValueObjects;

/// <summary>
/// Strongly-typed identifier for an order line item.
/// </summary>
/// <param name="Value">The underlying identifier value.</param>
public readonly record struct OrderItemId(Guid Value)
{
    /// <summary>
    /// Creates a new, unique <see cref="OrderItemId"/>.
    /// </summary>
    /// <returns>A new <see cref="OrderItemId"/> wrapping a freshly generated <see cref="Guid"/>.</returns>
    public static OrderItemId New() => new(Guid.NewGuid());
}
