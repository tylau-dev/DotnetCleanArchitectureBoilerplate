using Application.Common.Exceptions;
using Application.Orders.Commands;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using Moq;

namespace Application.Tests.Orders.Commands;

public sealed class CancelOrderHandlerTests
{
    [Fact]
    public async Task Should_CancelOrder_When_OrderIsDraft()
    {
        var order = Order.Create(CustomerId.New(), new Address("123 Main St", "Springfield", "12345", "USA"));

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new CancelOrderHandler(orderRepository.Object);

        await handler.Handle(new CancelOrderCommand(order.Id.Value), CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task Should_ThrowNotFoundException_When_OrderDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<OrderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new CancelOrderHandler(orderRepository.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new CancelOrderCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
