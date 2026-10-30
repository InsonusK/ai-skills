// Source: skill vp-grpc-client-in-dotnet

using System.Diagnostics;
using BuildingBlocks.Logging;
using Client.Abstractions;
using Client.Pipeline;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Client.Grpc;

/// <summary>
/// Runs around every unary call of a <see cref="GrpcApiClient"/>. It ends the
/// call after the timeout at the latest, sends the request id of the call
/// being served, and writes two records: "grpc client call sent" with the
/// request message as the payload, and "grpc client call answered" with the
/// status code, the duration, failed, and as the payload the response message
/// or, for a failed call, the status received and the request again. A failed
/// call throws <see cref="ApiUnavailableException"/> for UNAVAILABLE and
/// DEADLINE_EXCEEDED, <see cref="ApiStatusException"/> for any other status.
/// Streaming calls pass unchanged: the client variation point covers unary
/// calls only.
/// </summary>
public sealed class CallInterceptor : Interceptor
{
    /// <summary>Carries the request id of the call being served to the API.</summary>
    public const string RequestIdMetadata = "x-request-id";

    private readonly GrpcApiConfig _config;
    private readonly ILogger _logger;
    private readonly Dictionary<string, GrpcMethod> _methods;
    private readonly IReadOnlyList<IClientMiddleware> _middlewares;

    /// <summary>The interceptor of a client that calls only <paramref name="methods"/>, each through <paramref name="middlewares"/>, the first the outermost.</summary>
    public CallInterceptor(GrpcApiConfig config, ILogger logger, IReadOnlyList<GrpcMethod> methods, IReadOnlyList<IClientMiddleware> middlewares)
    {
        _config = config;
        _logger = logger;
        _methods = methods.ToDictionary(method => method.Name);
        _middlewares = middlewares;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var method = context.Method.FullName;
        if (!_methods.TryGetValue(method, out var listed))
        {
            var refused = Task.FromException<TResponse>(new InvalidOperationException($"{method} is no method of the adapter's list"));
            return new AsyncUnaryCall<TResponse>(refused, Task.FromResult(new Metadata()), () => Status.DefaultCancelled, () => [], () => { });
        }
        // The attempt the client middleware ran last: its headers, status, and trailers are the call's.
        AsyncUnaryCall<TResponse>? last = null;
        async Task<TResponse> RunAsync()
        {
            TResponse response = default!;
            await ClientPipeline.RunAsync(new ClientCall(_config.Name, method, listed.Retryable), _middlewares, async token =>
            {
                last = Attempt(request, context, continuation, token);
                response = await Answer(last, request, method, Stopwatch.GetTimestamp());
            }, context.Options.CancellationToken);
            return response;
        }
        var answered = RunAsync();
        async Task<Metadata> HeadersAsync()
        {
            try
            {
                await answered;
            }
            catch (Exception)
            {
                // the headers of a failed call are those of its last attempt
            }
            return last is null ? [] : await last.ResponseHeadersAsync;
        }
        return new AsyncUnaryCall<TResponse>(answered, HeadersAsync(),
            () => last?.GetStatus() ?? Status.DefaultSuccess, () => last?.GetTrailers() ?? [], () => last?.Dispose());
    }

    /// <summary>
    /// Sends the call once: its deadline the earlier of the caller's and the
    /// timeout, the request id of the call being served in its metadata, and
    /// the record "grpc client call sent".
    /// </summary>
    private AsyncUnaryCall<TResponse> Attempt<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation, CancellationToken cancellationToken)
        where TRequest : class
        where TResponse : class
    {
        var deadline = DateTime.UtcNow + _config.Timeout;
        if (context.Options.Deadline is { } callerDeadline && callerDeadline < deadline)
        {
            deadline = callerDeadline;
        }
        // A copy: the caller's metadata stays as it gave it.
        var headers = new Metadata();
        foreach (var entry in context.Options.Headers ?? [])
        {
            headers.Add(entry);
        }
        if (RequestId.Current is { } id)
        {
            headers.Add(RequestIdMetadata, id);
        }
        var options = context.Options.WithDeadline(deadline).WithHeaders(headers).WithCancellationToken(cancellationToken);
        _logger.Write(LogLevel.Information, "grpc client call sent",
            LogAttr.Of("api", _config.Name), LogAttr.Of("method", context.Method.FullName),
            LogAttr.Payload(new Dictionary<string, object?> { ["request"] = request }));
        return continuation(request, new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options));
    }

    private async Task<TResponse> Answer<TRequest, TResponse>(AsyncUnaryCall<TResponse> call, TRequest request, string method, long started)
    {
        var code = StatusCode.OK;
        var answered = new Dictionary<string, object?>();
        try
        {
            var response = await call.ResponseAsync;
            answered["response"] = response;
            return response;
        }
        catch (RpcException failed)
        {
            code = failed.StatusCode;
            answered["status"] = failed.Status;
            answered["request"] = request;
            throw code is StatusCode.Unavailable or StatusCode.DeadlineExceeded
                ? new ApiUnavailableException(_config.Name, $"{method}: {code}: {failed.Status.Detail}", failed)
                : new ApiStatusException(_config.Name, (ApiStatusCode)(int)code, failed.Status.Detail);
        }
        catch (Exception)
        {
            // Not a status of the API (a message the client cannot write):
            // logged as UNKNOWN, passed on as it is.
            code = StatusCode.Unknown;
            answered["request"] = request;
            throw;
        }
        finally
        {
            _logger.Write(LogLevel.Information, "grpc client call answered",
                LogAttr.Of("api", _config.Name), LogAttr.Of("method", method), LogAttr.Of("code", code.ToString()),
                LogAttr.Of("duration", Stopwatch.GetElapsedTime(started)), LogAttr.Failed(code != StatusCode.OK),
                LogAttr.Payload(answered));
        }
    }
}
