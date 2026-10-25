// Source: skill base-plateau-in-dotnet

using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Pipeline;

/// <summary>
/// The list of command middleware: the MediatR pipeline behaviors around every
/// command handler of every module, given by the composition root in one call.
/// Each lives in a folder of its own of the Middleware project.
/// </summary>
public static class CommandMiddleware
{
    /// <summary>
    /// Registers <paramref name="middlewares"/> in the order given, the first
    /// the outermost: it runs first on the way in and last on the way out
    /// (MediatR runs the behaviors in the order they were registered). Each is
    /// an open generic pipeline behavior - <c>typeof(LoggingMiddleware&lt;,&gt;)</c> -
    /// that MediatR closes for every command; one for some commands only asks
    /// the command for a mark (<c>command is IRestricted</c>) and passes every
    /// other command on unchanged.
    /// </summary>
    /// <exception cref="ArgumentException">A type that is not an open generic <c>IPipelineBehavior&lt;,&gt;</c>.</exception>
    public static IServiceCollection AddCommandMiddleware(this IServiceCollection services, params Type[] middlewares)
    {
        foreach (var middleware in middlewares)
        {
            var isBehavior = middleware.IsGenericTypeDefinition && middleware.GetInterfaces().Any(contract =>
                contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));
            if (!isBehavior)
            {
                throw new ArgumentException(
                    $"{middleware.Name} is not an open generic IPipelineBehavior<,>, as typeof(LoggingMiddleware<,>) is", nameof(middlewares));
            }
            services.AddTransient(typeof(IPipelineBehavior<,>), middleware);
        }
        return services;
    }
}
