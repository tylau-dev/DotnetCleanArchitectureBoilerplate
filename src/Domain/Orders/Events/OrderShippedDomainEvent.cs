using Domain.Common;
using Domain.Orders.ValueObjects;

namespace Domain.Orders.Events;

/// <summary>
/// Raised when an order transitions from <see cref="OrderStatus.Placed"/> to <see cref="OrderStatus.Shipped"/>.
/// </summary>
/// <param name="OrderId">The identifier of the order that was shipped.</param>
/// <param name="OccurredOnUtc">The UTC timestamp at which the event occurred.</param>
public sealed record OrderShippedDomainEvent(OrderId OrderId, DateTimeOffset OccurredOnUtc) : IDomainEvent;
