// Source: skill vp-http-client-in-dotnet

namespace Client.Pipeline;

/// <summary>One call of an operation of an API, as every client middleware sees it.</summary>
/// <param name="Api">How the log names the API: "todo-api".</param>
/// <param name="Operation">The operation: "POST /v1/items/plan" over HTTP, the full method name "/todo.v1.ItemsService/PlanItems" over gRPC.</param>
/// <param name="Retryable">True when the adapter marked the operation safe to send again: a repeat does what one call does.</param>
public sealed record ClientCall(string Api, string Operation, bool Retryable);

/// <summary>Sends the call once: one request, bounded by {API}_TIMEOUT, logged in its two records, ending or throwing a technical outcome.</summary>
public delegate Task ClientAttempt(CancellationToken cancellationToken);

/// <summary>
/// Runs around the attempts of a call: it calls next once to pass the call
/// on, more than once to repeat it, or not at all to refuse it. A variation
/// point (the retry) adds one to the client middleware of an API in the
/// composition root, without editing any client.
/// </summary>
public interface IClientMiddleware
{
    Task RunAsync(ClientCall call, ClientAttempt next, CancellationToken cancellationToken);
}

/// <summary>Runs the client middleware of an API around the attempts of one call.</summary>
public static class ClientPipeline
{
    /// <summary>Sends <paramref name="call"/> through <paramref name="middlewares"/>, the first the outermost, to <paramref name="attempt"/>.</summary>
    public static Task RunAsync(ClientCall call, IReadOnlyList<IClientMiddleware> middlewares, ClientAttempt attempt, CancellationToken cancellationToken)
    {
        var next = attempt;
        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = middlewares[i];
            var inner = next;
            next = token => middleware.RunAsync(call, inner, token);
        }
        return next(cancellationToken);
    }
}
