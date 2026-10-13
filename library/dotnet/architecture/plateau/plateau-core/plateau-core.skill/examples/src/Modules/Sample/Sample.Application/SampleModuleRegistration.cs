using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Application;

public static class SampleModuleRegistration
{
    public static IServiceCollection AddSampleModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SampleModuleRegistration).Assembly));
        services.AddValidatorsFromAssembly(typeof(SampleModuleRegistration).Assembly);
        services.AddSingleton<Features.Greet.GreetingStore>();
        return services;
    }
}
