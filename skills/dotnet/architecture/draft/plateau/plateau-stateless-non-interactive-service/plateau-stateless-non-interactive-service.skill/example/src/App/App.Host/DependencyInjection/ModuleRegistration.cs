using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Application.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterSampleModule(configuration);

        return services;
    }
}
