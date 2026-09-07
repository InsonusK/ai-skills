using App.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Shared.Repositories;

namespace App.Host.DependencyInjection;

public static class RepositoryRegistration
{
    // One open-generic registration serves every entity. Scoped, to share the DbContext per request.
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
        return services;
    }
}
