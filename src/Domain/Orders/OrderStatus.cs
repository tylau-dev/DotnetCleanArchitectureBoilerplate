namespace Domain.Orders;

/// <summary>
/// Represents the lifecycle status of an <see cref="Order"/>.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order has been created but not yet placed; items can still be added.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The order has been placed by the customer and is awaiting fulfillment.
    /// </summary>
    Placed = 1,

    /// <summary>
    /// The order has been shipped to the customer.
    /// </summary>
    Shipped = 2,

    /// <summary>
    /// The order has been cancelled and can no longer be modified.
    /// </summary>
    Cancelled = 3,
}
