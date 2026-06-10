using Domain.Common;
using Domain.Orders;
using Infrastructure.EventStore;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void Should_RegisterInfrastructureServices_When_AddInfrastructureCalled()
    {
        // The connection string here is a non-functional placeholder (see appsettings.json) used only to
        // satisfy AddInfrastructure's configuration requirement; no connection is ever opened.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // ApplicationDbContext depends on IPublisher (registered by Application's DI in the full
        // composition root); provide a stand-in here since this test exercises Infrastructure's
        // registrations in isolation.
        services.AddSingleton(Mock.Of<IPublisher>());

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<ApplicationDbContext>());
        Assert.NotNull(provider.GetRequiredService<IUnitOfWork>());
        Assert.NotNull(provider.GetRequiredService<IOrderRepository>());
        Assert.NotNull(provider.GetRequiredService<IEventStore>());
    }
}
