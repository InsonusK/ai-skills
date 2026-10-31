// Source: skill vp-postgres-in-dotnet

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Pipeline;

/// <summary>
/// The sender every entry point and handler receives once a variation point
/// needs what lives per command, such as a database context. An entry point
/// sends from outside any scope, so a command it sends opens one, runs in it, and
/// disposes it at its end; a command a handler sends finds that scope and runs
/// in it, which is how nested commands share one context and one unit of work.
/// Handlers of one command run one nested command at a time: a context serves
/// one call at a time.
/// </summary>
public sealed class ScopedSender(IServiceScopeFactory scopes) : ISender
{
    private static readonly AsyncLocal<IMediator?> Current = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        InScopeAsync(mediator => mediator.Send(request, cancellationToken));

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest =>
        InScopeAsync(async mediator =>
        {
            await mediator.Send(request, cancellationToken);
            return true;
        });

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        InScopeAsync(mediator => mediator.Send(request, cancellationToken));

    // A stream outlives the call that returns it, so no scope could end with it.
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("a command answers once: stream requests have no command scope");

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("a command answers once: stream requests have no command scope");

    private async Task<TResult> InScopeAsync<TResult>(Func<IMediator, Task<TResult>> send)
    {
        if (Current.Value is { } nested)
        {
            return await send(nested);
        }
        await using var scope = scopes.CreateAsyncScope();
        Current.Value = scope.ServiceProvider.GetRequiredService<IMediator>();
        try
        {
            return await send(Current.Value);
        }
        finally
        {
            Current.Value = null;
        }
    }
}

public static class CommandScope
{
    /// <summary>
    /// Makes <see cref="ScopedSender"/> the <see cref="ISender"/> of the
    /// application. Called after every module's <c>Add{Module}Module()</c>,
    /// which registers the sender it replaces.
    /// </summary>
    public static IServiceCollection AddCommandScope(this IServiceCollection services) =>
        services.Replace(ServiceDescriptor.Singleton<ISender, ScopedSender>());
}
