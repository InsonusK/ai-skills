using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Application;

public static class SampleApplicationRegistration
{
    public static IServiceCollection RegisterSampleModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(SampleApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof(SampleApplicationRegistration).Assembly);

        return services;
    }
}
