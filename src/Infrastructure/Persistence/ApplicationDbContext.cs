using System.Reflection;
using Domain.Common;
using Domain.Orders;
using Infrastructure.EventStore;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the relational persistence of domain aggregates,
/// and the <see cref="IUnitOfWork"/> implementation used to commit pending changes.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IEventStore eventStore;
    private readonly IPublisher publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options used to configure this context.</param>
    /// <param name="eventStore">The event store that domain events raised during <see cref="SaveChangesAsync"/> are appended to.</param>
    /// <param name="publisher">The publisher used to dispatch domain events raised during <see cref="SaveChangesAsync"/>.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IEventStore eventStore, IPublisher publisher)
        : base(options)
    {
        this.eventStore = eventStore;
        this.publisher = publisher;
    }

    /// <summary>
    /// Gets the set of <see cref="Order"/> aggregates.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Domain events raised by tracked aggregates are collected before the underlying
    /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> call (entity state may change after saving),
    /// cleared once the save succeeds, and then appended to the <see cref="IEventStore"/> and published via
    /// <see cref="IPublisher"/> as part of the same unit of work.
    /// </remarks>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entry in ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await eventStore.AppendAsync(domainEvent, cancellationToken).ConfigureAwait(false);
            await publisher.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }
}
