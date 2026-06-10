using Domain.Common;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using Infrastructure.EventStore;
using Infrastructure.Tests.TestSupport;
using MediatR;
using Moq;

namespace Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextTests
{
    [Fact]
    public async Task Should_CollectAndDispatchDomainEvents_When_SaveChangesAsyncCalled()
    {
        using var factory = new SqliteApplicationDbContextFactory();

        var eventStoreMock = new Mock<IEventStore>();
        var publisherMock = new Mock<IPublisher>();

        using var context = factory.CreateContext(eventStoreMock.Object, publisherMock.Object);

        var order = Order.Create(CustomerId.New(), new Address("1 Test St", "Testville", "00000", "USA"));
        order.AddItem(ProductId.New(), "Widget", new Money(5.00m, "USD"), 1);
        order.Place();

        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        Assert.Empty(order.DomainEvents);
        eventStoreMock.Verify(store => store.AppendAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        publisherMock.Verify(publisher => publisher.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Should_NotDispatchEvents_When_NoDomainEventsRaised()
    {
        using var factory = new SqliteApplicationDbContextFactory();

        var eventStoreMock = new Mock<IEventStore>();
        var publisherMock = new Mock<IPublisher>();

        using var context = factory.CreateContext(eventStoreMock.Object, publisherMock.Object);

        var order = Order.Create(CustomerId.New(), new Address("1 Test St", "Testville", "00000", "USA"));

        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        eventStoreMock.Verify(store => store.AppendAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        publisherMock.Verify(publisher => publisher.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
