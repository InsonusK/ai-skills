using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sample.Application.Concurrency;

namespace Sample.Application;

public static class SampleModuleRegistration
{
    public static IServiceCollection AddSampleModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SampleModuleRegistration).Assembly));
        services.AddValidatorsFromAssembly(typeof(SampleModuleRegistration).Assembly);

        // Per-entity concurrency version resolvers — the factory resolves these from the provider.
        services.AddScoped<TodoItemVersionResolver>();

        return services;
    }
}
