namespace Domain.Common;

/// <summary>
/// Base class for aggregate roots that track domain events raised during business operations.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> domainEvents = [];

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

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    /// <inheritdoc />
    public void ClearDomainEvents()
        => domainEvents.Clear();

    /// <summary>
    /// Records a domain event to be dispatched once the aggregate root's changes are persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => domainEvents.Add(domainEvent);
}
