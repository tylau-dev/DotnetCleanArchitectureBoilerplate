using Domain.Common;
using Domain.Orders.Events;
using Infrastructure.Common.Logging;
using Marten;
using Microsoft.Extensions.Logging;

namespace Infrastructure.EventStore;

/// <summary>
/// Marten-backed implementation of <see cref="IEventStore"/> that appends domain events to an
/// append-only audit log, keyed by the identifier of the aggregate that raised them.
/// </summary>
/// <remarks>
/// Each call to <see cref="AppendAsync"/> appends a single event and immediately calls
/// <see cref="IDocumentSession.SaveChangesAsync"/>, i.e. each append is its own Marten transaction.
/// This is acceptable for an audit log; batching multiple events into a single transaction is a
/// possible future optimization.
///
/// Resolving the event stream identifier currently requires a <c>switch</c> over the known
/// <c>Domain.Orders.Events</c> types. Extending the event store to cover additional bounded
/// contexts means extending <see cref="ResolveStreamId"/>. A future shared
/// <c>IAggregateEvent</c> marker interface exposing the aggregate identifier directly could
/// remove the need for this switch entirely; that is out of scope for now.
/// </remarks>
public sealed class MartenEventStore : IEventStore
{
    private readonly IDocumentSession _session;
    private readonly ILogger<MartenEventStore> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenEventStore"/> class.
    /// </summary>
    /// <param name="session">The Marten document session used to append events.</param>
    /// <param name="logger">The logger used to record appended events.</param>
    public MartenEventStore(IDocumentSession session, ILogger<MartenEventStore> logger)
    {
        _session = session;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var streamId = ResolveStreamId(domainEvent);

        _session.Events.Append(streamId, domainEvent);

        await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogAppendedDomainEvent(domainEvent.GetType().Name, streamId);
    }

    /// <summary>
    /// Resolves the event stream identifier for a given domain event, based on the aggregate that raised it.
    /// </summary>
    /// <param name="domainEvent">The domain event to resolve a stream identifier for.</param>
    /// <returns>The identifier of the aggregate that raised <paramref name="domainEvent"/>, or <see cref="Guid.Empty"/> if unknown.</returns>
    internal static Guid ResolveStreamId(IDomainEvent domainEvent) => domainEvent switch
    {
        OrderItemAddedDomainEvent e => e.OrderId.Value,
        OrderPlacedDomainEvent e => e.OrderId.Value,
        OrderShippedDomainEvent e => e.OrderId.Value,
        OrderCancelledDomainEvent e => e.OrderId.Value,
        _ => Guid.Empty,
    };
}
