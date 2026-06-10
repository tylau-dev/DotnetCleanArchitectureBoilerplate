namespace Domain.Common;

/// <summary>
/// Base class for aggregate roots that track domain events raised during business operations.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The aggregate root's unique identifier.</param>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <remarks>Reserved for EF Core materialization.</remarks>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Gets the domain events raised by this aggregate root that have not yet been dispatched.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Clears all domain events that have been raised by this aggregate root.
    /// </summary>
    public void ClearDomainEvents()
        => _domainEvents.Clear();

    /// <summary>
    /// Records a domain event to be dispatched once the aggregate root's changes are persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);
}
