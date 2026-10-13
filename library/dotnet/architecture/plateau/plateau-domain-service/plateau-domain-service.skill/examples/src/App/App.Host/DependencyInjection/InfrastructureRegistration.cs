using System.Reflection;
using App.Infrastructure.Concurrency;
using App.Infrastructure.Persistence;
using App.Infrastructure.UnitOfWork;
using BuildingBlocks.MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Application;
using Sample.Domain.Configurations;
using Shared.Concurrency;
using Shared.UnitOfWork;

namespace App.Host.DependencyInjection;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // The example uses the EF Core in-memory provider; a real deployment swaps in Npgsql here.
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("sample"));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<UnitOfWorkContext>();
        services.AddRepositories();

        var domainAssemblies = new[] { typeof(TodoItemConfig).Assembly };
        var applicationAssemblies = new[] { typeof(SampleModuleRegistration).Assembly };
        services.AddScoped<IEntityVersionResolverFactory>(sp =>
            new EntityVersionResolverFactory(sp, domainAssemblies, applicationAssemblies));

        return services;
    }
}
