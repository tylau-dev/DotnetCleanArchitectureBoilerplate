using Domain.Common;

namespace Domain.Orders.ValueObjects;

/// <summary>
/// Represents a postal shipping address.
/// </summary>
public sealed class Address : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> class.
    /// </summary>
    /// <param name="street">The street address, including house or unit number.</param>
    /// <param name="city">The city or locality.</param>
    /// <param name="postalCode">The postal or ZIP code.</param>
    /// <param name="country">The country.</param>
    public Address(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    /// <summary>
    /// Gets the street address, including house or unit number.
    /// </summary>
    public string Street { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the city or locality.
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the postal or ZIP code.
    /// </summary>
    public string PostalCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the country.
    /// </summary>
    public string Country { get; private set; } = string.Empty;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }
}
