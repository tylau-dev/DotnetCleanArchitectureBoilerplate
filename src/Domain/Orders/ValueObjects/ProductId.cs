namespace Domain.Orders.ValueObjects;

/// <summary>
/// Strongly-typed identifier referencing a product in the Product bounded context.
/// </summary>
/// <param name="Value">The underlying identifier value.</param>
public readonly record struct ProductId(Guid Value)
{
    /// <summary>
    /// Creates a new, unique <see cref="ProductId"/>.
    /// </summary>
    /// <returns>A new <see cref="ProductId"/> wrapping a freshly generated <see cref="Guid"/>.</returns>
    public static ProductId New() => new(Guid.NewGuid());
}
