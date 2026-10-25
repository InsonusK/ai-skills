// Source: skill base-plateau-in-dotnet

using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Pipeline;

/// <summary>
/// The list of command decorators: the MediatR pipeline behaviors around every
/// command handler of every module, given by the composition root in one call.
/// </summary>
public static class CommandDecorators
{
    /// <summary>
    /// Registers <paramref name="decorators"/> in the order given, the first
    /// the outermost: it runs first on the way in and last on the way out
    /// (MediatR runs the behaviors in the order they were registered). Each is
    /// an open generic pipeline behavior - <c>typeof(LoggingBehavior&lt;,&gt;)</c> -
    /// that MediatR closes for every command; one for some commands only asks
    /// the command for a mark (<c>command is IRestricted</c>) and passes every
    /// other command on unchanged.
    /// </summary>
    /// <exception cref="ArgumentException">A type that is not an open generic <c>IPipelineBehavior&lt;,&gt;</c>.</exception>
    public static IServiceCollection AddCommandDecorators(this IServiceCollection services, params Type[] decorators)
    {
        foreach (var decorator in decorators)
        {
            var isBehavior = decorator.IsGenericTypeDefinition && decorator.GetInterfaces().Any(contract =>
                contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));
            if (!isBehavior)
            {
                throw new ArgumentException(
                    $"{decorator.Name} is not an open generic IPipelineBehavior<,>, as typeof(LoggingBehavior<,>) is", nameof(decorators));
            }
            services.AddTransient(typeof(IPipelineBehavior<,>), decorator);
        }
        return services;
    }
}
