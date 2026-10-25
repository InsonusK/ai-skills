// Source: skill vp-http-api-in-dotnet

using System.Diagnostics;
using BuildingBlocks.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Http.Middleware.RequestLog;

/// <summary>
/// Writes two records per request, so the log reads top-down as the request
/// was served: "http request received" when it arrives, with the method, the
/// path, and as the payload what the client sent - the query, the headers,
/// and the body; then "http response sent" once it is answered, with the
/// status, the duration, failed (a status of 400 or above), and as the payload
/// the body as written, so a fault in turning a result into the response shows
/// in the log. A failed request's second record repeats what the client sent,
/// so it is in the log even when only failed records keep their payload.
/// It begins the request id of the call (<see cref="RequestId"/>), taken from
/// <see cref="RequestIdHeader"/> or made here, and sends it back. The request
/// body is read here and given back to the routes unchanged.
/// </summary>
public sealed class RequestLogMiddleware(RequestDelegate next, ILogger logger)
{
    /// <summary>Carries the id of a request: taken from the client when it sends one, else made here, and always sent back.</summary>
    public const string RequestIdHeader = "X-Request-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        var request = context.Request;
        using var requestId = RequestId.Begin(request.Headers[RequestIdHeader]);
        context.Response.Headers[RequestIdHeader] = RequestId.Current;

        request.EnableBuffering();
        var body = await new StreamReader(request.Body).ReadToEndAsync(context.RequestAborted);
        request.Body.Position = 0;
        var received = new Dictionary<string, object?>
        {
            ["query"] = request.Query.ToDictionary(pair => pair.Key, pair => pair.Value.ToString()),
            ["headers"] = request.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString()),
            ["body"] = body,
        };
        logger.Write(LogLevel.Information, "http request received",
            LogAttr.Of("method", request.Method), LogAttr.Of("path", request.Path.Value), LogAttr.Payload(received));

        // The response is written to the client and kept, to log it as sent.
        var client = context.Response.Body;
        using var sent = new MemoryStream();
        context.Response.Body = sent;
        try
        {
            await next(context);
        }
        finally
        {
            context.Response.Body = client;
            sent.Position = 0;
            await sent.CopyToAsync(client, context.RequestAborted);
        }

        var status = context.Response.StatusCode;
        var failed = status >= StatusCodes.Status400BadRequest;
        var answered = new Dictionary<string, object?>
        {
            ["response"] = new Dictionary<string, object?> { ["body"] = System.Text.Encoding.UTF8.GetString(sent.ToArray()) },
        };
        if (failed)
        {
            answered["request"] = received;
        }
        logger.Write(LogLevel.Information, "http response sent",
            LogAttr.Of("method", request.Method), LogAttr.Of("path", request.Path.Value),
            LogAttr.Of("status", status), LogAttr.Of("duration", Stopwatch.GetElapsedTime(started)),
            LogAttr.Failed(failed), LogAttr.Payload(answered));
    }
}
