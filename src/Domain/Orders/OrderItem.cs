using Domain.Common;
using Domain.Orders.ValueObjects;

namespace Domain.Orders;

/// <summary>
/// Represents a single line item within an <see cref="Order"/>.
/// </summary>
public sealed class OrderItem : Entity<OrderItemId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderItem"/> class.
    /// </summary>
    /// <param name="id">The line item's unique identifier.</param>
    /// <param name="productId">The identifier of the product this line item represents.</param>
    /// <param name="productName">A snapshot of the product's name at the time it was added.</param>
    /// <param name="unitPrice">The unit price of the product at the time it was added.</param>
    /// <param name="quantity">The quantity of the product ordered.</param>
    internal OrderItem(OrderItemId id, ProductId productId, string productName, Money unitPrice, int quantity)
        : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    // Reserved for EF Core materialization.
    private OrderItem()
    {
    }

    /// <summary>
    /// Gets the identifier of the product this line item represents.
    /// </summary>
    public ProductId ProductId { get; private set; }

    /// <summary>
    /// Gets the name of the product at the time it was added to the order.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unit price of the product at the time it was added to the order.
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    /// <summary>
    /// Gets the quantity of the product ordered.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the subtotal for this line item, calculated as <see cref="UnitPrice"/> multiplied by <see cref="Quantity"/>.
    /// </summary>
    public Money Subtotal => UnitPrice.Multiply(Quantity);
}
