using Domain.Orders;
using Domain.Orders.ValueObjects;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Tests.TestSupport;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Persistence.Configurations;

public sealed class OrderConfigurationTests
{
    [Fact]
    public async Task Should_PersistAndReloadOrderWithItems_When_SavedViaDbContext()
    {
        using var factory = new SqliteApplicationDbContextFactory();

        var order = Order.Create(CustomerId.New(), new Address("123 Main St", "Springfield", "12345", "USA"));
        order.AddItem(ProductId.New(), "Widget", new Money(9.99m, "USD"), 2);
        order.AddItem(ProductId.New(), "Gadget", new Money(19.99m, "USD"), 1);

        using (var writeContext = factory.CreateContext())
        {
            var repository = new OrderRepository(writeContext);
            await repository.AddAsync(order);
            await writeContext.SaveChangesAsync();
        }

        using var readContext = factory.CreateContext();
        var readRepository = new OrderRepository(readContext);
        var reloaded = await readRepository.GetByIdAsync(order.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(order.CustomerId, reloaded.CustomerId);
        Assert.Equal(order.Status, reloaded.Status);
        Assert.Equal(order.ShippingAddress, reloaded.ShippingAddress);
        Assert.Equal(2, reloaded.Items.Count);
        Assert.Contains(reloaded.Items, item => item.ProductName == "Widget" && item.UnitPrice == new Money(9.99m, "USD") && item.Quantity == 2);
        Assert.Contains(reloaded.Items, item => item.ProductName == "Gadget" && item.UnitPrice == new Money(19.99m, "USD") && item.Quantity == 1);
    }

    [Fact]
    public async Task Should_PersistShippingAddressAsOwnedType_When_OrderSaved()
    {
        using var factory = new SqliteApplicationDbContextFactory();

        var order = Order.Create(CustomerId.New(), new Address("456 Oak Ave", "Shelbyville", "67890", "USA"));

        using (var writeContext = factory.CreateContext())
        {
            await writeContext.Orders.AddAsync(order);
            await writeContext.SaveChangesAsync();
        }

        using var readContext = factory.CreateContext();
        await using var connection = (SqliteConnection)readContext.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT ShippingStreet, ShippingCity, ShippingPostalCode, ShippingCountry FROM Orders LIMIT 1";

        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal("456 Oak Ave", reader.GetString(0));
        Assert.Equal("Shelbyville", reader.GetString(1));
        Assert.Equal("67890", reader.GetString(2));
        Assert.Equal("USA", reader.GetString(3));
    }

    [Fact]
    public void Should_IgnoreComputedProperties_When_BuildingModel()
    {
        using var factory = new SqliteApplicationDbContextFactory();
        using var context = factory.CreateContext();

        var orderType = context.Model.FindEntityType(typeof(Order));
        var orderItemType = context.Model.FindEntityType(typeof(OrderItem));

        Assert.NotNull(orderType);
        Assert.NotNull(orderItemType);
        Assert.Null(orderType.FindProperty(nameof(Order.TotalAmount)));
        Assert.Null(orderType.FindProperty(nameof(Order.DomainEvents)));
        Assert.Null(orderItemType.FindProperty(nameof(OrderItem.Subtotal)));
    }
}
