namespace Domain.Common;

/// <summary>
/// Marker interface for domain events raised by aggregate roots.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp at which the event occurred.
    /// </summary>
    DateTimeOffset OccurredOnUtc { get; }
}
