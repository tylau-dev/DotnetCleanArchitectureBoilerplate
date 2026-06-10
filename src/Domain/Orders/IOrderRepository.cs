using Domain.Orders.ValueObjects;

namespace Domain.Orders;

/// <summary>
/// Persistence contract for <see cref="Order"/> aggregates.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Retrieves an order by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the order to retrieve.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The matching <see cref="Order"/>, or <see langword="null"/> if no order with the given identifier exists.</returns>
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new order to the repository.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an order with the given identifier exists.
    /// </summary>
    /// <param name="id">The identifier of the order to check.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns><see langword="true"/> if an order with the given identifier exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(OrderId id, CancellationToken cancellationToken = default);
}
