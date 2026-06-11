namespace Application.Orders.Dtos;

/// <summary>
/// Data transfer object representing the lifecycle status of an order.
/// </summary>
public enum OrderStatusDto
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
