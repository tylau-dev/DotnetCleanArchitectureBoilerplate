using Domain.Common;
using Domain.Orders;
using Domain.Orders.Events;
using Domain.Orders.ValueObjects;

namespace Domain.Tests.Orders;

public class OrderTests
{
    [Fact]
    public void Should_InitializeWithDraftStatusAndNoItems_When_Created()
    {
        // Act
        var order = Order.Create(CustomerId.New(), CreateAddress());

        // Assert
        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void Should_AddItemAndRaiseEvent_When_OrderIsDraft()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateAddress());
        var productId = ProductId.New();

        // Act
        order.AddItem(productId, "Widget", new Money(9.99m, "USD"), 2);

        // Assert
        Assert.Single(order.Items);

        var domainEvent = Assert.IsType<OrderItemAddedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal(productId, domainEvent.ProductId);
        Assert.Equal(2, domainEvent.Quantity);
    }

    [Fact]
    public void Should_ThrowInvalidOrderStateException_When_AddingItemOutsideDraft()
    {
        // Arrange
        var order = CreatePlacedOrder();

        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(
            () => order.AddItem(ProductId.New(), "Widget", new Money(9.99m, "USD"), 1));
    }

    [Fact]
    public void Should_ThrowInvalidOrderStateException_When_PlacingOrderWithNoItems()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateAddress());

        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(order.Place);
    }

    [Fact]
    public void Should_TransitionToPlacedAndRaiseEvent_When_PlacingDraftOrderWithItems()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateAddress());
        order.AddItem(ProductId.New(), "Widget", new Money(9.99m, "USD"), 1);
        order.ClearDomainEvents();

        // Act
        order.Place();

        // Assert
        Assert.Equal(OrderStatus.Placed, order.Status);

        var domainEvent = Assert.IsType<OrderPlacedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
    }

    [Fact]
    public void Should_TransitionToShippedAndRaiseEvent_When_ShippingPlacedOrder()
    {
        // Arrange
        var order = CreatePlacedOrder();
        order.ClearDomainEvents();

        // Act
        order.Ship();

        // Assert
        Assert.Equal(OrderStatus.Shipped, order.Status);

        var domainEvent = Assert.IsType<OrderShippedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
    }

    [Fact]
    public void Should_ThrowInvalidOrderStateException_When_ShippingDraftOrder()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateAddress());

        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(order.Ship);
    }

    [Fact]
    public void Should_ThrowInvalidOrderStateException_When_ShippingCancelledOrder()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateAddress());
        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(order.Ship);
    }

    [Fact]
    public void Should_TransitionToCancelledAndRaiseEvent_When_CancellingDraftOrder()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateAddress());
        order.ClearDomainEvents();

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);

        var domainEvent = Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
    }

    [Fact]
    public void Should_TransitionToCancelledAndRaiseEvent_When_CancellingPlacedOrder()
    {
        // Arrange
        var order = CreatePlacedOrder();
        order.ClearDomainEvents();

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public void Should_ThrowInvalidOrderStateException_When_CancellingShippedOrder()
    {
        // Arrange
        var order = CreatePlacedOrder();
        order.Ship();

        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(order.Cancel);
    }

    [Fact]
    public void Should_ThrowInvalidOrderStateException_When_CancellingAlreadyCancelledOrder()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateAddress());
        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(order.Cancel);
    }

    [Fact]
    public void Should_SumItemSubtotals_When_CalculatingTotalAmount()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateAddress());
        order.AddItem(ProductId.New(), "Widget", new Money(10.00m, "USD"), 2);
        order.AddItem(ProductId.New(), "Gadget", new Money(5.00m, "USD"), 3);

        // Act
        var total = order.TotalAmount;

        // Assert
        Assert.Equal(new Money(35.00m, "USD"), total);
    }

    private static Address CreateAddress()
        => new("123 Main St", "Springfield", "12345", "USA");

    private static Order CreatePlacedOrder()
    {
        var order = Order.Create(CustomerId.New(), CreateAddress());
        order.AddItem(ProductId.New(), "Widget", new Money(9.99m, "USD"), 1);
        order.Place();

        return order;
    }
}
