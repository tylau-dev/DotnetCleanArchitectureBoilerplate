using Application.Orders.Dtos;
using Application.Orders.Mappings;
using AutoMapper;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Orders.Mappings;

public sealed class OrderMappingProfileTests
{
    [Fact]
    public void Should_HaveValidConfiguration()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>(), NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Should_MapOrderToOrderDto()
    {
        var order = Order.Create(CustomerId.New(), new Address("123 Main St", "Springfield", "12345", "USA"));
        order.AddItem(ProductId.New(), "Widget", new Money(9.99m, "USD"), 2);

        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>(), NullLoggerFactory.Instance);
        var mapper = configuration.CreateMapper();

        var dto = mapper.Map<OrderDto>(order);

        Assert.Equal(order.Id.Value, dto.Id);
        Assert.Equal(order.CustomerId.Value, dto.CustomerId);
        Assert.Equal(OrderStatusDto.Draft, dto.Status);
        Assert.Equal(order.ShippingAddress.Street, dto.ShippingAddress.Street);

        var item = Assert.Single(dto.Items);
        Assert.Equal(9.99m, item.UnitPrice);
        Assert.Equal("USD", item.Currency);
        Assert.Equal(19.98m, item.Subtotal);

        Assert.Equal(19.98m, dto.TotalAmount);
        Assert.Equal("USD", dto.Currency);
    }
}
