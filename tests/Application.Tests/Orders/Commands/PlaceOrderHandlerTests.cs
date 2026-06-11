using Application.Common.Exceptions;
using Application.Orders.Commands;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using Moq;

namespace Application.Tests.Orders.Commands;

public sealed class PlaceOrderHandlerTests
{
    [Fact]
    public async Task Should_PlaceOrder_When_OrderExistsAndHasItems()
    {
        var order = Order.Create(CustomerId.New(), new Address("123 Main St", "Springfield", "12345", "USA"));
        order.AddItem(ProductId.New(), "Widget", new Money(9.99m, "USD"), 1);

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new PlaceOrderHandler(orderRepository.Object);

        await handler.Handle(new PlaceOrderCommand(order.Id.Value), CancellationToken.None);

        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public async Task Should_ThrowNotFoundException_When_OrderDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<OrderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new PlaceOrderHandler(orderRepository.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new PlaceOrderCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
