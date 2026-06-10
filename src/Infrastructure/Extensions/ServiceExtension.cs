using Domain.Common;
using Domain.Orders;
using Infrastructure.EventStore;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

/// <summary>
/// Registers Infrastructure-layer services: the EF Core <see cref="ApplicationDbContext"/>,
/// repositories, and the Marten-backed event store.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Adds the Infrastructure layer's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configuration">The configuration containing the <c>Database</c> connection string.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "event_store";
        }).UseLightweightSessions();

        services.AddScoped<IEventStore, MartenEventStore>();

        return services;
    }
}
