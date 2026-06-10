using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IOrderRepository"/>.
/// </summary>
public sealed class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used to access <see cref="Order"/> aggregates.</param>
    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
        => _dbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await _dbContext.Orders.AddAsync(order, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<bool> ExistsAsync(OrderId id, CancellationToken cancellationToken = default)
        => _dbContext.Orders.AnyAsync(order => order.Id == id, cancellationToken);
}
