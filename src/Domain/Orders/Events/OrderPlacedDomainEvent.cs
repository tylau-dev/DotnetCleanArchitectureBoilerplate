using Domain.Common;
using Domain.Orders.ValueObjects;

namespace Domain.Orders.Events;

/// <summary>
/// Raised when an order transitions from <see cref="OrderStatus.Draft"/> to <see cref="OrderStatus.Placed"/>.
/// </summary>
/// <param name="OrderId">The identifier of the order that was placed.</param>
/// <param name="OccurredOnUtc">The UTC timestamp at which the event occurred.</param>
public sealed record OrderPlacedDomainEvent(OrderId OrderId, DateTimeOffset OccurredOnUtc) : IDomainEvent;
