using Application.Orders.Commands;
using Domain.Orders;
using Moq;

namespace Application.Tests.Orders.Commands;

public sealed class CreateOrderHandlerTests
{
    [Fact]
    public async Task Should_CreateAndAddOrder_When_RequestIsValid()
    {
        var orderRepository = new Mock<IOrderRepository>();
        Order? addedOrder = null;
        orderRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => addedOrder = order)
            .Returns(Task.CompletedTask);

        var handler = new CreateOrderHandler(orderRepository.Object);
        var command = new CreateOrderCommand(Guid.NewGuid(), "123 Main St", "Springfield", "12345", "USA");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(addedOrder);
        Assert.Equal(result, addedOrder!.Id.Value);
        Assert.Equal(command.CustomerId, addedOrder.CustomerId.Value);
        Assert.Equal(OrderStatus.Draft, addedOrder.Status);
        Assert.Equal(command.Street, addedOrder.ShippingAddress.Street);
        orderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
