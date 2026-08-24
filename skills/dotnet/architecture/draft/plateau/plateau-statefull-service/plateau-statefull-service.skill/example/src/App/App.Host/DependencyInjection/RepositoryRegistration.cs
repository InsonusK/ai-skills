using App.Infrastructure.Repositories;
using App.Infrastructure.UnitOfWork;
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Repositories;
using Shared.UnitOfWork;

namespace App.Host.DependencyInjection;

public static class RepositoryRegistration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UnitOfWorkContext>();

        return services;
    }
}
