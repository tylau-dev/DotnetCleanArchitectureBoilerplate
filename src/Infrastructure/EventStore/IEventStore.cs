using Domain.Common;

namespace Infrastructure.EventStore;

/// <summary>
/// Append-only audit log for domain events raised by aggregates.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends a domain event to the event store.
    /// </summary>
    /// <param name="domainEvent">The domain event to append.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
