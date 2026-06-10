using Domain.Common;
using Domain.Orders.ValueObjects;

namespace Domain.Orders.Events;

/// <summary>
/// Raised when a line item is added to an order.
/// </summary>
/// <param name="OrderId">The identifier of the order the item was added to.</param>
/// <param name="ProductId">The identifier of the product that was added.</param>
/// <param name="Quantity">The quantity of the product that was added.</param>
/// <param name="OccurredOnUtc">The UTC timestamp at which the event occurred.</param>
public sealed record OrderItemAddedDomainEvent(OrderId OrderId, ProductId ProductId, int Quantity, DateTimeOffset OccurredOnUtc) : IDomainEvent;
