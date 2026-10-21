// Source: skill vp-http-api-in-dotnet

using System.Text.Encodings.Web;
using System.Text.Json;
using App.Host.Api.Errors;
using BuildingBlocks.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace App.Host.Api.Http.Pipeline;

/// <summary>
/// Answers every exception a route lets out with a google.rpc.Status body,
/// so a route is written for its success alone:
/// <list type="number">
/// <item>a request that cannot be bound (a body that is not JSON) - 400 INVALID_ARGUMENT, the handler never called;</item>
/// <item>an <see cref="ErrorKindException"/> - its kind's code and status by <see cref="RpcStatus"/>, with its message;</item>
/// <item>anything else - 500 INTERNAL "internal error", the exception and its stack only in the log;</item>
/// <item>an error ASP.NET answers itself with no body - no route (404), another method (405) - its
/// status with a google.rpc.Status body; a body that is not declared JSON (415) is 400
/// INVALID_ARGUMENT, as every request that cannot be decoded.</item>
/// </list>
/// A route never answers an error itself (<c>Results.BadRequest</c>, ProblemDetails).
/// </summary>
public sealed class ErrorAnswers(RequestDelegate next, ILogger logger)
{
    // A message keeps its quotes readable: "a \"b\"", not "a \u0022b\u0022".
    private static readonly JsonSerializerOptions StatusJson = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest && context.Response.ContentType is null && !context.Response.HasStarted)
            {
                await WriteFrameworkErrorAsync(context);
            }
        }
        catch (Exception error) when (!context.Response.HasStarted)
        {
            var (code, message) = error is BadHttpRequestException badRequest
                ? (RpcCode.InvalidArgument, DecodeMessage(badRequest))
                : RpcStatus.Of(error);
            if (code == RpcCode.Internal)
            {
                logger.Write(LogLevel.Error, "http request failed", error,
                    LogAttr.Of("method", context.Request.Method), LogAttr.Of("path", context.Request.Path.Value));
            }
            await WriteStatusAsync(context.Response, code, message);
        }
    }

    // What a client is told of a request that could not be bound: the JSON
    // parser's reason, or that the body is missing.
    private static string DecodeMessage(BadHttpRequestException error) => error.InnerException is JsonException json
        ? "request body is not valid JSON: " + json.Message
        : "request body is missing or not valid JSON";

    // An error status ASP.NET set without a body, given the body every error has.
    private static Task WriteFrameworkErrorAsync(HttpContext context)
    {
        var request = $"{context.Request.Method} {context.Request.Path}";
        var (code, message) = context.Response.StatusCode switch
        {
            StatusCodes.Status415UnsupportedMediaType => (RpcCode.InvalidArgument, "request body is not JSON: Content-Type must be application/json"),
            StatusCodes.Status404NotFound => (RpcCode.NotFound, $"no route for {request}"),
            StatusCodes.Status405MethodNotAllowed => (RpcCode.Unimplemented, $"method not allowed: {request}"),
            StatusCodes.Status401Unauthorized => (RpcCode.Unauthenticated, "unauthenticated"),
            StatusCodes.Status403Forbidden => (RpcCode.PermissionDenied, "permission denied"),
            >= StatusCodes.Status500InternalServerError => (RpcCode.Internal, RpcStatus.InternalMessage),
            _ => (RpcCode.InvalidArgument, $"request rejected with status {context.Response.StatusCode}"),
        };
        // 404 and 405 keep their status; every other answer is the code's.
        var status = code is RpcCode.NotFound or RpcCode.Unimplemented ? context.Response.StatusCode : code.HttpStatus();
        return WriteStatusAsync(context.Response, code, message, status);
    }

    /// <summary>Writes google.rpc.Status in proto-JSON with the code's HTTP status.</summary>
    public static async Task WriteStatusAsync(HttpResponse response, RpcCode code, string message, int? status = null)
    {
        response.Clear();
        response.StatusCode = status ?? code.HttpStatus();
        response.ContentType = "application/json";
        await response.WriteAsync(JsonSerializer.Serialize(new { code = (int)code, message, details = Array.Empty<object>() }, StatusJson));
    }
}
