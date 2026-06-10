using MediatR;

namespace Infrastructure.Persistence;

/// <summary>
/// No-op <see cref="IPublisher"/> used where a real MediatR publisher is unavailable, such as
/// EF Core design-time tooling.
/// </summary>
internal sealed class NullPublisher : IPublisher
{
    /// <inheritdoc />
    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => Task.CompletedTask;
}
