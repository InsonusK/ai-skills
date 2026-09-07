using BuildingBlocks.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class PipelineRegistration
{
    // The single ordered list of pipeline behaviors.
    // Exception handling wraps everything; validation short-circuits invalid requests;
    // concurrency guards stale updates; unit-of-work commits once, last.
    public static IServiceCollection AddPipeline(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GuidResolvingBehavior<,>)); // VP6 — idempotent create
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
        return services;
    }
}
