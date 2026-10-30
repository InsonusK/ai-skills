// Source: skill vp-client-retry-in-dotnet

using System.Runtime.ExceptionServices;
using BuildingBlocks.Logging;
using Client.Abstractions;
using Client.Pipeline;
using Microsoft.Extensions.Logging;

namespace Retry;

/// <summary>
/// Sends a retryable call again while its attempt throws
/// <see cref="ApiUnavailableException"/>, up to <see cref="RetryConfig.Attempts"/>
/// attempts in all. Each attempt has the whole {API}_TIMEOUT of the client;
/// the caller's cancellation ends a wait, and the call throws the outcome of
/// its last attempt. Before each repeat a WARN record "api client call
/// retried" names the API, the operation, the attempt that failed, the wait,
/// and its error. A call that is not retryable, or that ends any other way,
/// passes on unchanged - a repeat of a call the API answered could do its
/// work twice.
/// </summary>
public sealed class RetryMiddleware(RetryConfig config, ILogger logger) : IClientMiddleware
{
    public async Task RunAsync(ClientCall call, ClientAttempt next, CancellationToken cancellationToken)
    {
        if (!call.Retryable)
        {
            await next(cancellationToken);
            return;
        }
        var backoff = config.Backoff;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await next(cancellationToken);
                return;
            }
            catch (ApiUnavailableException unavailable) when (attempt < config.Attempts)
            {
                var wait = Jitter(backoff);
                logger.Write(LogLevel.Warning, "api client call retried",
                    LogAttr.Of("api", call.Api), LogAttr.Of("operation", call.Operation),
                    LogAttr.Of("attempt", attempt), LogAttr.Of("wait", wait), LogAttr.Of("error", unavailable.Message));
                try
                {
                    await Task.Delay(wait, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    ExceptionDispatchInfo.Throw(unavailable); // with the stack of the attempt that threw it
                }
                backoff *= 2;
            }
        }
    }

    /// <summary>A wait between half and one and a half of backoff, so the clients an outage stopped together do not all call again at once.</summary>
    private static TimeSpan Jitter(TimeSpan backoff) => backoff / 2 + backoff * Random.Shared.NextDouble();
}
