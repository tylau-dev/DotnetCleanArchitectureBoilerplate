namespace Application.Orders.Dtos;

/// <summary>
/// Data transfer object representing a postal shipping address.
/// </summary>
public sealed record AddressDto
{
    /// <summary>Gets the street address, including house or unit number.</summary>
    public required string Street { get; init; }

    /// <summary>Gets the city or locality.</summary>
    public required string City { get; init; }

    /// <summary>Gets the postal or ZIP code.</summary>
    public required string PostalCode { get; init; }

    /// <summary>Gets the country.</summary>
    public required string Country { get; init; }
}
