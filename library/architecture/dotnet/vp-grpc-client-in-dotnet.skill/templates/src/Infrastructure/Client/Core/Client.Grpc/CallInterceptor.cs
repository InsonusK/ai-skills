// Source: skill vp-grpc-client-in-dotnet

using System.Diagnostics;
using BuildingBlocks.Logging;
using Client.Abstractions;
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
public sealed class CallInterceptor(GrpcApiConfig config, ILogger logger) : Interceptor
{
    /// <summary>Carries the request id of the call being served to the API.</summary>
    public const string RequestIdMetadata = "x-request-id";

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        // The earlier of the caller's deadline and the timeout.
        var deadline = DateTime.UtcNow + config.Timeout;
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
        var options = context.Options.WithDeadline(deadline).WithHeaders(headers);
        var method = context.Method.FullName;
        logger.Write(LogLevel.Information, "grpc client call sent",
            LogAttr.Of("api", config.Name), LogAttr.Of("method", method),
            LogAttr.Payload(new Dictionary<string, object?> { ["request"] = request }));
        var started = Stopwatch.GetTimestamp();
        var call = continuation(request, new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options));
        return new AsyncUnaryCall<TResponse>(Answer(call, request, method, started),
            call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
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
                ? new ApiUnavailableException(config.Name, $"{method}: {code}: {failed.Status.Detail}", failed)
                : new ApiStatusException(config.Name, (ApiStatusCode)(int)code, failed.Status.Detail);
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
            logger.Write(LogLevel.Information, "grpc client call answered",
                LogAttr.Of("api", config.Name), LogAttr.Of("method", method), LogAttr.Of("code", code.ToString()),
                LogAttr.Of("duration", Stopwatch.GetElapsedTime(started)), LogAttr.Failed(code != StatusCode.OK),
                LogAttr.Payload(answered));
        }
    }
}
