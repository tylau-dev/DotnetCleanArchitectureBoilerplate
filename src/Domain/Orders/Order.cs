using Domain.Common;
using Domain.Orders.Events;
using Domain.Orders.ValueObjects;

namespace Domain.Orders;

/// <summary>
/// Aggregate root representing a customer's order, including its line items, shipping address,
/// and lifecycle status.
/// </summary>
public sealed class Order : AggregateRoot<OrderId>
{
    /// <summary>
    /// The currency assumed for <see cref="TotalAmount"/> while the order has no line items yet.
    /// </summary>
    private const string DefaultCurrency = "USD";

    private readonly List<OrderItem> _items = [];

    private Order(OrderId id, CustomerId customerId, Address shippingAddress)
        : base(id)
    {
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    // Reserved for EF Core materialization.
    private Order()
    {
    }

    /// <summary>
    /// Gets the identifier of the customer who placed the order.
    /// </summary>
    public CustomerId CustomerId { get; private set; }

    /// <summary>
    /// Gets the current lifecycle status of the order.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Gets the address the order will be shipped to.
    /// </summary>
    public Address ShippingAddress { get; private set; } = null!;

    /// <summary>
    /// Gets the line items that make up this order.
    /// </summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets the UTC timestamp at which the order was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the total amount of the order, calculated as the sum of all line item subtotals.
    /// </summary>
    public Money TotalAmount =>
        _items.Count == 0
            ? Money.Zero(DefaultCurrency)
            : _items.Select(item => item.Subtotal).Aggregate((total, subtotal) => total.Add(subtotal));

    /// <summary>
    /// Creates a new order in <see cref="OrderStatus.Draft"/> status with no line items.
    /// </summary>
    /// <param name="customerId">The identifier of the customer placing the order.</param>
    /// <param name="shippingAddress">The address the order will be shipped to.</param>
    /// <returns>A new <see cref="Order"/> instance.</returns>
    public static Order Create(CustomerId customerId, Address shippingAddress)
        => new(OrderId.New(), customerId, shippingAddress);

    /// <summary>
    /// Adds a line item to the order.
    /// </summary>
    /// <param name="productId">The identifier of the product being added.</param>
    /// <param name="productName">A snapshot of the product's name at the time it is added.</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    /// <param name="quantity">The quantity of the product being added. Must be greater than zero.</param>
    /// <exception cref="InvalidOrderStateException">Thrown when the order is not in <see cref="OrderStatus.Draft"/> status.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quantity"/> is not greater than zero, or <paramref name="unitPrice"/> is negative.</exception>
    public void AddItem(ProductId productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOrderStateException($"Cannot add items to an order in '{Status}' status.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantity, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice.Amount);

        var item = new OrderItem(OrderItemId.New(), productId, productName, unitPrice, quantity);
        _items.Add(item);

        RaiseDomainEvent(new OrderItemAddedDomainEvent(Id, productId, quantity, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Places the order, transitioning it from <see cref="OrderStatus.Draft"/> to <see cref="OrderStatus.Placed"/>.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Thrown when the order is not in <see cref="OrderStatus.Draft"/> status, or has no line items.</exception>
    public void Place()
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOrderStateException($"Cannot place an order in '{Status}' status.");
        }

        if (_items.Count == 0)
        {
            throw new InvalidOrderStateException("Cannot place an order with no items.");
        }

        Status = OrderStatus.Placed;

        RaiseDomainEvent(new OrderPlacedDomainEvent(Id, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Ships the order, transitioning it from <see cref="OrderStatus.Placed"/> to <see cref="OrderStatus.Shipped"/>.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Thrown when the order is not in <see cref="OrderStatus.Placed"/> status.</exception>
    public void Ship()
    {
        if (Status != OrderStatus.Placed)
        {
            throw new InvalidOrderStateException($"Cannot ship an order in '{Status}' status.");
        }

        Status = OrderStatus.Shipped;

        RaiseDomainEvent(new OrderShippedDomainEvent(Id, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Cancels the order, transitioning it to <see cref="OrderStatus.Cancelled"/>.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Thrown when the order is already <see cref="OrderStatus.Shipped"/> or <see cref="OrderStatus.Cancelled"/>.</exception>
    public void Cancel()
    {
        if (Status is not (OrderStatus.Draft or OrderStatus.Placed))
        {
            throw new InvalidOrderStateException($"Cannot cancel an order in '{Status}' status.");
        }

        Status = OrderStatus.Cancelled;

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, DateTimeOffset.UtcNow));
    }
}
