using Domain.Orders.Events;
using Domain.Orders.ValueObjects;
using Infrastructure.EventStore;

namespace Infrastructure.Tests.EventStore;

/// <remarks>
/// True Marten round-trip verification requires a live PostgreSQL instance via
/// <c>docker-compose up -d</c> and is out of scope for this automated suite.
/// </remarks>
public sealed class MartenEventStoreTests
{
    [Fact]
    public void Should_ResolveOrderIdAsStreamId_When_GivenEachOrderDomainEventType()
    {
        var orderId = OrderId.New();
        var occurredOnUtc = DateTimeOffset.UtcNow;

        Assert.Equal(orderId.Value, MartenEventStore.ResolveStreamId(new OrderItemAddedDomainEvent(orderId, ProductId.New(), 1, occurredOnUtc)));
        Assert.Equal(orderId.Value, MartenEventStore.ResolveStreamId(new OrderPlacedDomainEvent(orderId, occurredOnUtc)));
        Assert.Equal(orderId.Value, MartenEventStore.ResolveStreamId(new OrderShippedDomainEvent(orderId, occurredOnUtc)));
        Assert.Equal(orderId.Value, MartenEventStore.ResolveStreamId(new OrderCancelledDomainEvent(orderId, occurredOnUtc)));
    }
}
