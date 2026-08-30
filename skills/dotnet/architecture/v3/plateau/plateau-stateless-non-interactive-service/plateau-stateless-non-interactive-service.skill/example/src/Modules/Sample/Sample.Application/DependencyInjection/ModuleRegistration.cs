using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Application.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection RegisterSampleModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add module-specific services here.
        return services;
    }
}
