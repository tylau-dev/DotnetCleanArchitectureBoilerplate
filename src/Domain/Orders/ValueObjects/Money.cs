using Domain.Common;

namespace Domain.Orders.ValueObjects;

/// <summary>
/// Represents a monetary amount in a specific currency.
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The ISO 4217 currency code.</param>
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets the ISO 4217 currency code.
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a zero-value <see cref="Money"/> instance in the given currency.
    /// </summary>
    /// <param name="currency">The ISO 4217 currency code.</param>
    /// <returns>A <see cref="Money"/> instance representing zero in the specified currency.</returns>
    public static Money Zero(string currency) => new(0m, currency);

    /// <summary>
    /// Adds another monetary amount to this one.
    /// </summary>
    /// <param name="other">The amount to add.</param>
    /// <returns>A new <see cref="Money"/> instance representing the sum.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="other"/> has a different currency.</exception>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another monetary amount from this one.
    /// </summary>
    /// <param name="other">The amount to subtract.</param>
    /// <returns>A new <see cref="Money"/> instance representing the difference.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="other"/> has a different currency.</exception>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies this monetary amount by a quantity.
    /// </summary>
    /// <param name="quantity">The quantity to multiply by.</param>
    /// <returns>A new <see cref="Money"/> instance representing the product.</returns>
    public Money Multiply(int quantity) => new(Amount * quantity, Currency);

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Cannot perform this operation on money values with different currencies: '{Currency}' and '{other.Currency}'.");
        }
    }
}
