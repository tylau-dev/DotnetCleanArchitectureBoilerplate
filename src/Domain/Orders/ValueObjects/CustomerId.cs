namespace Domain.Orders.ValueObjects;

/// <summary>
/// Strongly-typed identifier referencing a customer in the Customer bounded context.
/// </summary>
/// <param name="Value">The underlying identifier value.</param>
public readonly record struct CustomerId(Guid Value)
{
    /// <summary>
    /// Creates a new, unique <see cref="CustomerId"/>.
    /// </summary>
    /// <returns>A new <see cref="CustomerId"/> wrapping a freshly generated <see cref="Guid"/>.</returns>
    public static CustomerId New() => new(Guid.NewGuid());
}
