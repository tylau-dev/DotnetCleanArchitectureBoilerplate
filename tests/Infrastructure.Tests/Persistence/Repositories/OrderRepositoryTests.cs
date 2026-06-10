using Domain.Orders;
using Domain.Orders.ValueObjects;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Tests.TestSupport;

namespace Infrastructure.Tests.Persistence.Repositories;

public sealed class OrderRepositoryTests
{
    [Fact]
    public async Task Should_ReturnNull_When_OrderDoesNotExist()
    {
        using var factory = new SqliteApplicationDbContextFactory();
        using var context = factory.CreateContext();
        var repository = new OrderRepository(context);

        var result = await repository.GetByIdAsync(OrderId.New());

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnOrderWithItems_When_OrderExists()
    {
        using var factory = new SqliteApplicationDbContextFactory();

        var order = Order.Create(CustomerId.New(), new Address("1 Test St", "Testville", "00000", "USA"));
        order.AddItem(ProductId.New(), "Widget", new Money(5.00m, "USD"), 3);

        using (var seedContext = factory.CreateContext())
        {
            var seedRepository = new OrderRepository(seedContext);
            await seedRepository.AddAsync(order);
            await seedContext.SaveChangesAsync();
        }

        using var readContext = factory.CreateContext();
        var readRepository = new OrderRepository(readContext);
        var result = await readRepository.GetByIdAsync(order.Id);

        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Single(result.Items);
        Assert.Equal("Widget", result.Items.First().ProductName);
    }

    [Fact]
    public async Task Should_ReturnTrue_When_OrderExists()
    {
        using var factory = new SqliteApplicationDbContextFactory();

        var order = Order.Create(CustomerId.New(), new Address("1 Test St", "Testville", "00000", "USA"));

        using (var seedContext = factory.CreateContext())
        {
            await seedContext.Orders.AddAsync(order);
            await seedContext.SaveChangesAsync();
        }

        using var readContext = factory.CreateContext();
        var readRepository = new OrderRepository(readContext);

        Assert.True(await readRepository.ExistsAsync(order.Id));
    }

    [Fact]
    public async Task Should_ReturnFalse_When_OrderDoesNotExist()
    {
        using var factory = new SqliteApplicationDbContextFactory();
        using var context = factory.CreateContext();
        var repository = new OrderRepository(context);

        Assert.False(await repository.ExistsAsync(OrderId.New()));
    }
}
