// Source: skill vp-grpc-api-in-dotnet

using System.Diagnostics;
using BuildingBlocks.Logging;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Grpc.Middleware.CallLog;

/// <summary>
/// Writes two records per call, so the log reads top-down as the call was
/// served: "grpc call received" when it arrives, with the method and the
/// request message as the payload; then "grpc call answered" with the status
/// code, the duration, failed (any code but OK), and the response message as
/// returned as the payload, so a fault in turning a result into the response
/// shows in the log. A failed call's second record repeats the request, so it
/// is in the log even when only failed records keep their payload. It begins
/// the request id of the call (<see cref="RequestId"/>), taken from the
/// <see cref="RequestIdMetadata"/> metadata or made here, and sends it back in
/// the response headers. It runs outside AnswerFailuresMiddleware, so a failure is logged
/// with the code it was answered with.
/// </summary>
public sealed class CallLogMiddleware(ILogger logger) : Interceptor
{
    /// <summary>The metadata key that carries the id of a call.</summary>
    public const string RequestIdMetadata = "x-request-id";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var started = Stopwatch.GetTimestamp();
        using var requestId = RequestId.Begin(context.RequestHeaders.GetValue(RequestIdMetadata));
        await context.WriteResponseHeadersAsync(new Metadata { { RequestIdMetadata, RequestId.Current! } });
        logger.Write(LogLevel.Information, "grpc call received",
            LogAttr.Of("method", context.Method), LogAttr.Payload(new Dictionary<string, object?> { ["request"] = request }));

        TResponse? response = default;
        var code = StatusCode.OK;
        try
        {
            response = await continuation(request, context);
            return response;
        }
        catch (RpcException error)
        {
            code = error.StatusCode;
            throw;
        }
        catch (Exception)
        {
            code = StatusCode.Unknown;
            throw;
        }
        finally
        {
            var failed = code != StatusCode.OK;
            var answered = new Dictionary<string, object?> { ["response"] = response };
            if (failed)
            {
                answered["request"] = request;
            }
            logger.Write(LogLevel.Information, "grpc call answered",
                LogAttr.Of("method", context.Method), LogAttr.Of("code", code.ToString()),
                LogAttr.Of("duration", Stopwatch.GetElapsedTime(started)), LogAttr.Failed(failed), LogAttr.Payload(answered));
        }
    }
}
