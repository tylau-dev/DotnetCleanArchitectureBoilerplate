using Domain.Common;

namespace Infrastructure.EventStore;

/// <summary>
/// No-op <see cref="IEventStore"/> used where a real event store is unavailable, such as
/// EF Core design-time tooling.
/// </summary>
internal sealed class NullEventStore : IEventStore
{
    /// <inheritdoc />
    public Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
