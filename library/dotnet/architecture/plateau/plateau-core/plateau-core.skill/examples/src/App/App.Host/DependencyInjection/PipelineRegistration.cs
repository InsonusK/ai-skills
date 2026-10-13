using BuildingBlocks.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    // The single ordered list of pipeline behaviors. Exception handling wraps everything;
    // validation runs before handlers. VP behaviors (concurrency, guid, unit-of-work) insert
    // themselves at named anchor positions in deeper plateaus.
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
