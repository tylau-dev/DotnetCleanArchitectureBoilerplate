using Application.Orders.Dtos;
using Application.Orders.Mappings;
using Application.Orders.Queries;
using AutoMapper;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.Tests.Orders.Queries;

public sealed class GetOrderByIdHandlerTests
{
    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>(), NullLoggerFactory.Instance);

        return configuration.CreateMapper();
    }

    [Fact]
    public async Task Should_ReturnMappedOrder_When_OrderExists()
    {
        var order = Order.Create(CustomerId.New(), new Address("123 Main St", "Springfield", "12345", "USA"));
        order.AddItem(ProductId.New(), "Widget", new Money(9.99m, "USD"), 2);

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new GetOrderByIdHandler(orderRepository.Object, CreateMapper());

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id.Value), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(order.Id.Value, result!.Id);
        Assert.Equal(order.CustomerId.Value, result.CustomerId);
        Assert.Equal(OrderStatusDto.Draft, result.Status);
        Assert.Single(result.Items);
        Assert.Equal(19.98m, result.TotalAmount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task Should_ReturnNull_When_OrderDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<OrderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new GetOrderByIdHandler(orderRepository.Object, CreateMapper());

        var result = await handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
