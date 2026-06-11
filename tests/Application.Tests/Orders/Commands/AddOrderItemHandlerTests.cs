using Application.Common.Exceptions;
using Application.Orders.Commands;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using Moq;

namespace Application.Tests.Orders.Commands;

public sealed class AddOrderItemHandlerTests
{
    [Fact]
    public async Task Should_AddItemToOrder_When_OrderExists()
    {
        var order = Order.Create(CustomerId.New(), new Address("123 Main St", "Springfield", "12345", "USA"));
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new AddOrderItemHandler(orderRepository.Object);
        var command = new AddOrderItemCommand(order.Id.Value, Guid.NewGuid(), "Widget", 9.99m, "USD", 2);

        await handler.Handle(command, CancellationToken.None);

        var item = Assert.Single(order.Items);
        Assert.Equal(command.ProductId, item.ProductId.Value);
        Assert.Equal("Widget", item.ProductName);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public async Task Should_ThrowNotFoundException_When_OrderDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<OrderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new AddOrderItemHandler(orderRepository.Object);
        var command = new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Widget", 9.99m, "USD", 2);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
