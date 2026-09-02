using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Logging;

namespace BuildingBlocks.MediatR;

// Registered first in the pipeline. Catches every unhandled exception, logs it Critical,
// and returns a generic Result.Error with no exception detail.
public sealed class ExceptionHandlingBehavior<TRequest, TResponse>(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Critical, LogEvents.UnhandledException, ex, "Unhandled exception handling {Request}", typeof(TRequest).Name);
            var error = typeof(TResponse).GetMethod("Error", [typeof(string)])!;
            return (TResponse)error.Invoke(null, ["An unexpected error occurred."])!;
        }
    }
}
