using Infrastructure.EventStore;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Infrastructure.Tests.TestSupport;

/// <summary>
/// Provides <see cref="ApplicationDbContext"/> instances backed by a single open in-memory SQLite
/// connection, so multiple contexts can share the same database within a test.
/// </summary>
public sealed class SqliteApplicationDbContextFactory : IDisposable
{
    private readonly SqliteConnection connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteApplicationDbContextFactory"/> class,
    /// opening an in-memory SQLite connection and creating the schema.
    /// </summary>
    public SqliteApplicationDbContextFactory()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates a new <see cref="ApplicationDbContext"/> over the shared in-memory connection.
    /// </summary>
    /// <param name="eventStore">The event store to use, or <see langword="null"/> for a mock.</param>
    /// <param name="publisher">The publisher to use, or <see langword="null"/> for a mock.</param>
    /// <returns>A new <see cref="ApplicationDbContext"/> instance.</returns>
    public ApplicationDbContext CreateContext(IEventStore? eventStore = null, IPublisher? publisher = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options, eventStore ?? Mock.Of<IEventStore>(), publisher ?? Mock.Of<IPublisher>());
    }

    /// <inheritdoc />
    public void Dispose() => connection.Dispose();
}
