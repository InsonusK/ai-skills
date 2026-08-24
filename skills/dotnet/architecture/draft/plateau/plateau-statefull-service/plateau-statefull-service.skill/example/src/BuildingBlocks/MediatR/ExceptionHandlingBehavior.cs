using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.MediatR;

public class ExceptionHandlingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "Unhandled exception while handling request {RequestType}.",
                typeof(TRequest).Name);

            return (TResponse)Result.Error("An unexpected error occurred. Please try again later.");
        }
    }
}
