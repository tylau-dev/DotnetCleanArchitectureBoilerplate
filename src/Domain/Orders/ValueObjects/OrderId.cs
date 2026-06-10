namespace Domain.Orders.ValueObjects;

/// <summary>
/// Strongly-typed identifier for an order aggregate root.
/// </summary>
/// <param name="Value">The underlying identifier value.</param>
public readonly record struct OrderId(Guid Value)
{
    /// <summary>
    /// Creates a new, unique <see cref="OrderId"/>.
    /// </summary>
    /// <returns>A new <see cref="OrderId"/> wrapping a freshly generated <see cref="Guid"/>.</returns>
    public static OrderId New() => new(Guid.NewGuid());
}
