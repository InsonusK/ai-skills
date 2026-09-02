using Microsoft.Extensions.DependencyInjection;
using Sample.Application;

namespace App.Host.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddModules(this IServiceCollection services)
    {
        services.AddSampleModule();
        return services;
    }
}
