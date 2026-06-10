using System.Reflection;
using Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

/// <summary>
/// Registers Application-layer services: MediatR handlers and pipeline behaviors,
/// FluentValidation validators, and AutoMapper profiles.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Adds the Application layer's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(_ => { }, assembly);

        return services;
    }
}
