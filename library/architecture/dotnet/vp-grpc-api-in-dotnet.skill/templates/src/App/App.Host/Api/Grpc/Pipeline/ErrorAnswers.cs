// Source: skill vp-grpc-api-in-dotnet

using App.Host.Api.Errors;
using BuildingBlocks.Logging;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Grpc.Pipeline;

/// <summary>
/// Answers every exception a method lets out by its kind through the one
/// table of <see cref="RpcStatus"/>, so a method is written for its success
/// alone: an <c>ErrorKindException</c> - its kind's code with its message (a
/// request that cannot become the command throws one of kind invalid); an
/// <see cref="RpcException"/> as it is; anything else INTERNAL "internal
/// error", the exception and its stack only in the log. ASP.NET's gRPC server
/// already keeps serving after an exception; this decides what the client is told.
/// </summary>
public sealed class ErrorAnswers(ILogger logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception error) when (error is not RpcException)
        {
            var (code, message) = RpcStatus.Of(error);
            if (code == RpcCode.Internal)
            {
                logger.Write(LogLevel.Error, "grpc call failed", error, LogAttr.Of("method", context.Method));
            }
            throw new RpcException(new Status((StatusCode)code, message));
        }
    }
}
