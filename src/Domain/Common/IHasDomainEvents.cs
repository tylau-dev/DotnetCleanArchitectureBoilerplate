namespace Domain.Common;

/// <summary>
/// Exposes the domain events raised by an entity, independent of its identifier type.
/// </summary>
/// <remarks>
/// Allows infrastructure components (e.g. a <c>DbContext.SaveChangesAsync</c> override) to collect
/// and dispatch domain events from tracked aggregates without depending on
/// <see cref="AggregateRoot{TId}"/>'s generic type parameter.
/// </remarks>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the domain events raised by this entity that have not yet been dispatched.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events that have been raised by this entity.
    /// </summary>
    void ClearDomainEvents();
}
