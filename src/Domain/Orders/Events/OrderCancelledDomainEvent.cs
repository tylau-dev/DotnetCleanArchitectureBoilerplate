using Domain.Common;
using Domain.Orders.ValueObjects;

namespace Domain.Orders.Events;

/// <summary>
/// Raised when an order is cancelled.
/// </summary>
/// <param name="OrderId">The identifier of the order that was cancelled.</param>
/// <param name="OccurredOnUtc">The UTC timestamp at which the event occurred.</param>
public sealed record OrderCancelledDomainEvent(OrderId OrderId, DateTimeOffset OccurredOnUtc) : IDomainEvent;
