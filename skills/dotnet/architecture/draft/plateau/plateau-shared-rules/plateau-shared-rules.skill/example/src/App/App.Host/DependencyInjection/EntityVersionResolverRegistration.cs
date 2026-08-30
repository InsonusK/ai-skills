using App.Infrastructure.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using Sample.Application;
using Shared.Concurrency;

namespace App.Host.DependencyInjection;

public static class EntityVersionResolverRegistration
{
    public static IServiceCollection AddEntityVersionResolvers(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(SampleApplicationRegistration).Assembly)
            .AddClasses(c => c.AssignableTo<IEntityVersionResolver>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IEntityVersionResolverFactory, EntityVersionResolverFactory>();

        return services;
    }
}
